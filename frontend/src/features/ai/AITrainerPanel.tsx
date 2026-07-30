import { useCallback, useEffect, useId, useRef, useState } from 'react';
import { Badge, Button, Card, CardBody, Icons, Input, Spinner } from '@/components/ui';
import { aiTrainerService } from '@/services';
import { toApiError } from '@/api/errors';
import { notify } from '@/utils/toast';
import { cn } from '@/utils/cn';
import { loadAnam, resolveEvents, type AnamClient } from './anamClient';

type Status = 'idle' | 'connecting' | 'live' | 'error';

interface TranscriptLine {
  id: string;
  role: 'user' | 'ai';
  text: string;
}

interface Props {
  moduleId?: string | null;
  trainingId?: string | null;
  subjectLabel?: string | null;
  personaName?: string | null;
  compact?: boolean;
}

export function AITrainerPanel({ moduleId, trainingId, subjectLabel, personaName, compact }: Props) {
  const videoId = useId().replace(/:/g, '');
  const clientRef = useRef<AnamClient | null>(null);
  const sessionTokenRef = useRef<string | null>(null);

  const [status, setStatus] = useState<Status>('idle');
  const [error, setError] = useState<string | null>(null);
  const [micOn, setMicOn] = useState(true);
  const [speakerOn, setSpeakerOn] = useState(true);
  const [question, setQuestion] = useState('');
  const [asking, setAsking] = useState(false);
  const [transcript, setTranscript] = useState<TranscriptLine[]>([]);
  const [collapsed, setCollapsed] = useState(compact ?? false);
  const [primedSubject, setPrimedSubject] = useState<string | null>(null);
  const subject = primedSubject ?? subjectLabel ?? null;

  const stop = useCallback(async () => {
    const token = sessionTokenRef.current;
    try {
      clientRef.current?.stopStreaming?.();
    } catch {
    }
    clientRef.current = null;
    const video = document.getElementById(videoId) as HTMLVideoElement | null;
    if (video) video.srcObject = null;
    if (token) {
      sessionTokenRef.current = null;
      await aiTrainerService.stopSession(token).catch(() => undefined);
    }
    setStatus('idle');
  }, [videoId]);

  useEffect(() => {
    return () => {
      void stop();
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const start = async () => {
    setStatus('connecting');
    setError(null);
    setTranscript([]);
    try {
      const session = await aiTrainerService.startSession({
        moduleId: moduleId ?? null,
        trainingId: trainingId ?? null,
        personaName: personaName ?? null,
      });
      sessionTokenRef.current = session.sessionToken;
      setPrimedSubject(session.subjectTitle ?? null);

      const mod = await loadAnam();
      const events = resolveEvents(mod);
      const client = mod.createClient(session.sessionToken);
      clientRef.current = client;

      client.addListener(events.SESSION_READY, () => setStatus('live'));
      client.addListener(events.CONNECTION_CLOSED, () => setStatus('idle'));
      const onHistory = (payload: unknown) => {
        if (Array.isArray(payload)) {
          setTranscript(
            payload
              .filter((m): m is { role: string; content: string } => !!m && typeof m === 'object')
              .map((m, i) => ({
                id: String(i),
                role: m.role === 'user' ? 'user' : 'ai',
                text: m.content ?? '',
              })),
          );
        }
      };
      client.addListener(events.MESSAGE_HISTORY_UPDATED, onHistory);

      await client.streamToVideoElement(videoId);
      setStatus((s) => (s === 'connecting' ? 'live' : s));
    } catch (err) {
      const message = toApiError(err).message;
      setError(
        message.includes('Failed to fetch') || message.includes('import')
          ? 'Could not load the AI avatar SDK. Check your network connection and that the Anam service is configured on the server.'
          : message,
      );
      setStatus('error');
      sessionTokenRef.current = null;
    }
  };

  const ask = async () => {
    const text = question.trim();
    if (!text) return;
    setAsking(true);
    setTranscript((t) => [...t, { id: crypto.randomUUID(), role: 'user', text }]);
    setQuestion('');
    try {
      if (clientRef.current?.talk) {
        await clientRef.current.talk(text);
      }
      const res = await aiTrainerService.ask({
        sessionToken: sessionTokenRef.current ?? '',
        question: text,
        moduleId: moduleId ?? null,
        trainingId: trainingId ?? null,
      });
      if (res.live && res.answer) {
        setTranscript((t) => [...t, { id: crypto.randomUUID(), role: 'ai', text: res.answer }]);
      }
    } catch (err) {
      notify.apiError(err);
    } finally {
      setAsking(false);
    }
  };

  const toggleMic = () => {
    const client = clientRef.current;
    setMicOn((on) => {
      const next = !on;
      try {
        if (next) client?.unmuteInputAudio?.();
        else client?.muteInputAudio?.();
      } catch {
      }
      return next;
    });
  };

  const toggleSpeaker = () => {
    const video = document.getElementById(videoId) as HTMLVideoElement | null;
    setSpeakerOn((on) => {
      const next = !on;
      if (video) video.muted = !next;
      return next;
    });
  };

  const isLive = status === 'live';
  const isConnecting = status === 'connecting';

  return (
    <Card className={cn('overflow-hidden', compact && 'border-violet-200/80')}>
      <div className="flex items-center justify-between gap-3 border-b border-ink-100 bg-violet-50/40 px-4 py-3.5">
        <div className="flex min-w-0 items-center gap-3">
          <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-violet-100 text-violet-700 ring-1 ring-inset ring-violet-200/70">
            <Icons.ai size={19} />
          </span>
          <div className="min-w-0">
            <p className="truncate text-sm font-bold text-ink-900">
              AI Trainer
              {subject && <span className="font-medium text-ink-500"> · {subject}</span>}
            </p>
            <StatusPill status={status} />
          </div>
        </div>
        <div className="flex items-center gap-2">
          {compact && (
            <Button
              variant="ghost"
              size="icon"
              className="h-9 w-9"
              onClick={() => setCollapsed((c) => !c)}
              aria-label={collapsed ? 'Expand AI trainer' : 'Collapse AI trainer'}
            >
              <Icons.chevronDown size={18} className={cn('transition-transform', !collapsed && 'rotate-180')} />
            </Button>
          )}
        </div>
      </div>

      {!collapsed && (
        <CardBody className="space-y-4">
          <div className="relative aspect-video w-full overflow-hidden rounded-2xl bg-shell ring-1 ring-inset ring-ink-900/10">
            {/* eslint-disable-next-line jsx-a11y/media-has-caption */}
            <video id={videoId} autoPlay playsInline className="h-full w-full object-cover" />
            {status !== 'live' && (
              <div className="absolute inset-0 flex flex-col items-center justify-center gap-3.5 bg-shell text-center text-white/70">
                <span
                  className="pointer-events-none absolute inset-0"
                  style={{
                    background:
                      'radial-gradient(22rem 12rem at 50% 30%, rgb(139 92 246 / 0.20), transparent 70%)',
                  }}
                  aria-hidden
                />
                {isConnecting ? (
                  <>
                    <Spinner size={26} />
                    <p className="relative text-[13px] font-semibold">Connecting to your AI trainer…</p>
                  </>
                ) : status === 'error' ? (
                  <>
                    <Icons.alert size={28} className="relative text-rose-400" />
                    <p className="relative max-w-xs px-6 text-[13px] leading-relaxed">{error}</p>
                  </>
                ) : (
                  <>
                    <span className="relative flex h-14 w-14 items-center justify-center rounded-2xl bg-white/10 text-white ring-1 ring-inset ring-white/15">
                      <Icons.ai size={28} />
                    </span>
                    <p className="relative max-w-xs px-6 text-[13px] leading-relaxed">
                      {subject
                        ? `Start a session and the avatar will tutor you on ${subject}.`
                        : 'Start a session to talk with your AI trainer.'}
                    </p>
                  </>
                )}
              </div>
            )}
            {isLive && (
              <div className="absolute left-3 top-3">
                <Badge className="bg-rose-500 text-white ring-rose-600/30">
                  <span className="h-1.5 w-1.5 animate-pulse rounded-full bg-white" /> LIVE
                </Badge>
              </div>
            )}
          </div>

          <div className="flex items-center gap-2">
            {isLive ? (
              <Button variant="danger" leftIcon={<Icons.stop size={16} />} onClick={stop}>
                End session
              </Button>
            ) : (
              <Button leftIcon={<Icons.play size={16} />} loading={isConnecting} onClick={start}>
                Start session
              </Button>
            )}
            <div className="flex-1" />
            <Button
              variant="outline"
              size="icon"
              onClick={toggleMic}
              disabled={!isLive}
              className={cn(!micOn && 'text-rose-500')}
              aria-label={micOn ? 'Mute microphone' : 'Unmute microphone'}
              title={micOn ? 'Mute microphone' : 'Unmute microphone'}
            >
              {micOn ? <Icons.mic size={18} /> : <Icons.micOff size={18} />}
            </Button>
            <Button
              variant="outline"
              size="icon"
              onClick={toggleSpeaker}
              disabled={!isLive}
              className={cn(!speakerOn && 'text-rose-500')}
              aria-label={speakerOn ? 'Mute speaker' : 'Unmute speaker'}
              title={speakerOn ? 'Mute speaker' : 'Unmute speaker'}
            >
              {speakerOn ? <Icons.speaker size={18} /> : <Icons.speakerOff size={18} />}
            </Button>
          </div>

          {transcript.length > 0 && (
            <div className="max-h-60 space-y-2.5 overflow-y-auto rounded-xl border border-ink-200/70 bg-ink-50/70 p-3.5">
              {transcript.map((line) => (
                <div key={line.id} className={cn('flex', line.role === 'user' ? 'justify-end' : 'justify-start')}>
                  <span
                    className={cn(
                      'max-w-[82%] rounded-2xl px-3.5 py-2.5 text-[13.5px] leading-relaxed',
                      line.role === 'user'
                        ? 'bg-brand-700 text-white'
                        : 'border border-ink-200/70 bg-white text-ink-700 shadow-card',
                    )}
                  >
                    {line.text}
                  </span>
                </div>
              ))}
            </div>
          )}

          <form
            className="flex items-center gap-2"
            onSubmit={(e) => {
              e.preventDefault();
              void ask();
            }}
          >
            <Input
              value={question}
              onChange={(e) => setQuestion(e.target.value)}
              placeholder={isLive ? 'Ask anything about this material…' : 'Start a session to ask questions'}
              disabled={!isLive || asking}
              className="flex-1"
            />
            <Button type="submit" size="icon" loading={asking} disabled={!isLive || !question.trim()} aria-label="Send">
              <Icons.send size={18} />
            </Button>
          </form>
        </CardBody>
      )}
    </Card>
  );
}

function StatusPill({ status }: { status: Status }) {
  const map: Record<Status, { label: string; cls: string }> = {
    idle: { label: 'Ready when you are', cls: 'text-ink-400' },
    connecting: { label: 'Connecting…', cls: 'text-amber-600' },
    live: { label: 'Live session', cls: 'text-green-600' },
    error: { label: 'Connection failed', cls: 'text-rose-600' },
  };
  const s = map[status];
  return (
    <span className={cn('mt-0.5 flex items-center gap-1.5 text-[11.5px] font-semibold', s.cls)}>
      <span className={cn('h-1.5 w-1.5 rounded-full bg-current', status === 'live' && 'animate-pulse')} />
      {s.label}
    </span>
  );
}
