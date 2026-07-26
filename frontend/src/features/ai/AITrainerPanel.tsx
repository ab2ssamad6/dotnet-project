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
  personaName?: string | null;
  compact?: boolean;
}

/**
 * Reusable AI Trainer panel. Fetches a streaming session token from the API,
 * connects the Anam.ai avatar to a <video> element, and offers Start/Stop,
 * ask-a-question, mic and speaker controls with full status + error handling.
 */
export function AITrainerPanel({ moduleId, personaName, compact }: Props) {
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

  const stop = useCallback(async () => {
    const token = sessionTokenRef.current;
    try {
      clientRef.current?.stopStreaming?.();
    } catch {
      /* ignore */
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

  // Ensure the session is torn down on unmount.
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
      const session = await aiTrainerService.startSession({ moduleId: moduleId ?? null, personaName: personaName ?? null });
      sessionTokenRef.current = session.sessionToken;

      const mod = await loadAnam();
      const events = resolveEvents(mod);
      const client = mod.createClient(session.sessionToken);
      clientRef.current = client;

      client.addListener(events.SESSION_READY, () => setStatus('live'));
      client.addListener(events.CONNECTION_CLOSED, () => setStatus('idle'));
      // Capture streamed conversation messages when the SDK provides them.
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
      // Some SDK versions never emit SESSION_READY before streaming resolves.
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
      // Drive the avatar to speak client-side when supported…
      if (clientRef.current?.talk) {
        await clientRef.current.talk(text);
      }
      // …and fetch a textual answer from the backend for the transcript.
      const res = await aiTrainerService.ask({
        sessionToken: sessionTokenRef.current ?? '',
        question: text,
        moduleId: moduleId ?? null,
      });
      if (res.answer) {
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
        /* ignore */
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
    <Card className={cn(compact && 'border-violet-200')}>
      <div className="flex items-center justify-between border-b border-slate-100 px-4 py-3">
        <div className="flex items-center gap-2.5">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-violet-100 text-violet-600">
            <Icons.ai size={20} />
          </span>
          <div>
            <p className="text-sm font-semibold text-slate-800">AI Trainer</p>
            <StatusPill status={status} />
          </div>
        </div>
        <div className="flex items-center gap-2">
          {compact && (
            <Button variant="ghost" size="icon" onClick={() => setCollapsed((c) => !c)} aria-label="Toggle panel">
              <Icons.chevronDown size={18} className={cn('transition-transform', !collapsed && 'rotate-180')} />
            </Button>
          )}
        </div>
      </div>

      {!collapsed && (
        <CardBody className="space-y-4">
          {/* Avatar video */}
          <div className="relative aspect-video w-full overflow-hidden rounded-xl bg-slate-900">
            {/* eslint-disable-next-line jsx-a11y/media-has-caption */}
            <video id={videoId} autoPlay playsInline className="h-full w-full object-cover" />
            {status !== 'live' && (
              <div className="absolute inset-0 flex flex-col items-center justify-center gap-3 bg-slate-900 text-center text-slate-300">
                {isConnecting ? (
                  <>
                    <Spinner size={28} />
                    <p className="text-sm">Connecting to your AI trainer…</p>
                  </>
                ) : status === 'error' ? (
                  <>
                    <Icons.alert size={30} className="text-rose-400" />
                    <p className="max-w-xs px-4 text-sm">{error}</p>
                  </>
                ) : (
                  <>
                    <span className="flex h-14 w-14 items-center justify-center rounded-full bg-white/10">
                      <Icons.ai size={30} />
                    </span>
                    <p className="text-sm">Start a session to chat with your AI trainer.</p>
                  </>
                )}
              </div>
            )}
            {isLive && (
              <div className="absolute left-3 top-3">
                <Badge className="bg-rose-500 text-white">
                  <span className="mr-0.5 h-1.5 w-1.5 animate-pulse rounded-full bg-white" /> LIVE
                </Badge>
              </div>
            )}
          </div>

          {/* Controls */}
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

          {/* Transcript */}
          {transcript.length > 0 && (
            <div className="max-h-56 space-y-2 overflow-y-auto rounded-lg border border-slate-100 bg-slate-50 p-3">
              {transcript.map((line) => (
                <div key={line.id} className={cn('flex', line.role === 'user' ? 'justify-end' : 'justify-start')}>
                  <span
                    className={cn(
                      'max-w-[80%] rounded-2xl px-3 py-2 text-sm',
                      line.role === 'user' ? 'bg-brand-600 text-white' : 'bg-white text-slate-700 shadow-sm',
                    )}
                  >
                    {line.text}
                  </span>
                </div>
              ))}
            </div>
          )}

          {/* Ask a question */}
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
              placeholder={isLive ? 'Ask the AI trainer a question…' : 'Start a session to ask questions'}
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
    idle: { label: 'Ready', cls: 'text-slate-400' },
    connecting: { label: 'Connecting…', cls: 'text-amber-500' },
    live: { label: 'Live', cls: 'text-emerald-500' },
    error: { label: 'Error', cls: 'text-rose-500' },
  };
  const s = map[status];
  return (
    <span className={cn('flex items-center gap-1 text-xs font-medium', s.cls)}>
      <span className="h-1.5 w-1.5 rounded-full bg-current" />
      {s.label}
    </span>
  );
}
