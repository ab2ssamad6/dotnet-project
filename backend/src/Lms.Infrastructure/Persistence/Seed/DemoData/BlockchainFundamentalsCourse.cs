using Lms.Domain.Entities;
using Lms.Domain.Enums;

using static Lms.Infrastructure.Persistence.Seed.DemoData.DemoContent;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

public static class BlockchainFundamentalsCourse
{
    public const string Title = "Blockchain Fundamentals";

    public static Training Create(Category category, Trainer trainer) => NewTraining(
        Title,
        "How distributed ledgers actually work, from hash functions and Merkle trees up to consensus, smart "
            + "contracts and decentralised applications. The course is deliberately even-handed: you will learn "
            + "what blockchains genuinely solve, what they cost, and when a database is the better answer.",
        DifficultyLevel.Intermediate,
        category,
        trainer,
        Primitives(),
        Consensus(),
        BitcoinAndEthereum(),
        SmartContracts(),
        TokensAndRisk(),
        FinalAssessment());

    private static Module Primitives() => NewModule(
        "Distributed Ledgers and Cryptographic Primitives",
        "The double-spend problem, and the hash functions, Merkle trees and signatures that solve it.",
        60,
        aiAvatarEnabled: true,
        NewLesson(
            "The problem a blockchain solves",
            """
            Digital information copies perfectly. That is wonderful for documents and fatal for money: if a
            balance is just a number in a file, nothing stops the same coin being spent twice. Traditional
            finance solves this with a trusted central party - a bank whose ledger is authoritative by
            agreement.

            A blockchain answers a narrower question: can a group of participants who do not trust one another,
            and who have no central authority, agree on a single ordered history of transactions?

            The design that emerged has three parts. Every participant keeps a full copy of the ledger, so
            there is no single record to corrupt. Every entry is cryptographically signed, so only the holder
            of a key can move their own funds. And a consensus rule decides which version of history is
            canonical when copies disagree.

            The cost of this is worth stating plainly. Replicating all data to every participant, and paying
            for consensus, makes a blockchain enormously slower and more expensive than a database. Bitcoin
            settles a handful of transactions per second; a single Postgres instance handles tens of thousands.
            You accept that cost only when removing the trusted third party is genuinely the point.

            Not every chain is open. Permissionless networks let anyone join and validate. Permissioned
            networks restrict participation to known parties, which buys speed and privacy while giving up much
            of the censorship resistance - and often leaves you with a slow database and extra ceremony.
            """),
        NewLesson(
            "Hashes, Merkle trees and signatures",
            """
            A cryptographic hash function maps input of any size to a fixed-size output. SHA-256 always
            produces 256 bits. Three properties make it useful here: it is deterministic, so the same input
            always yields the same digest; it is one-way, so the input cannot be recovered from the digest; and
            it exhibits the avalanche effect, so changing a single bit of input changes roughly half the output
            bits unpredictably.

            Hashing is not encryption. There is no key and nothing to decrypt. Blockchain data is not secret -
            on a public chain every transaction is visible to everyone. What hashing provides is tamper
            evidence, not confidentiality.

            Blocks are chained by including the previous block's hash in the current block's header. Alter a
            transaction in block 500 and its hash changes, so block 501's recorded previous-hash no longer
            matches, and so on to the tip. Rewriting history therefore means recomputing every block after the
            one you touched, faster than the rest of the network extends the honest chain.

            A Merkle tree hashes transactions in pairs, then hashes those hashes, repeatedly, until one root
            remains. That single root in the block header commits to every transaction in the block. Its
            practical payoff is the inclusion proof: you can prove a transaction is in a block by supplying
            only the log-many sibling hashes along its path, rather than the whole block. Light clients rely on
            this.

            Digital signatures supply authorisation. The holder of a private key signs a transaction; anyone
            with the matching public key can verify the signature without learning the private key. That is
            what proves a spend was authorised by the owner.
            """),
        NewExercise(
            "Trace a tampered block",
            """
            A chain has blocks 1 to 10. An attacker edits one transaction amount in block 5.

            1. Which blocks' hashes become invalid, and why?
            2. What exactly must the attacker recompute to present a chain that internally validates?
            3. Why is this economically hard on a large proof-of-work network?
            4. If the transaction were removed rather than edited, what else in block 5's header changes?
            5. Explain why "the data is encrypted" is the wrong description of this protection.
            """,
            """
            Block 5's hash changes, which invalidates block 6's stored previous-hash, and the break cascades to
            block 10. The attacker must re-mine blocks 5 through 10 and then keep pace with the honest network,
            which requires a majority of hash power - the work is deliberately expensive and must be redone
            faster than everyone else combined. Removing a transaction also changes the Merkle root. Nothing
            here is encrypted: the data is public, and hashing provides tamper evidence rather than secrecy.
            """),
        NewQuiz(
            "Primitives check",
            Choice(
                "What links one block to the previous one?",
                ("The previous block's hash, stored in the current block's header", true),
                ("A sequential database identifier", false),
                ("The miner's public key", false),
                ("A shared timestamp server", false)),
            TrueFalse(
                "Data on a public blockchain is encrypted and therefore unreadable by other participants.",
                false),
            Choice(
                "The Merkle root in a block header:",
                ("Commits to every transaction in the block and enables compact inclusion proofs", true),
                ("Stores the miner's reward address", false),
                ("Encrypts the transaction list", false),
                ("Records the block's position in the chain", false)),
            Choice(
                "The avalanche effect means that changing one bit of input:",
                ("Changes roughly half the output bits unpredictably", true),
                ("Changes exactly one output bit", false),
                ("Leaves the hash unchanged", false),
                ("Makes the hash longer", false)),
            Multiple(
                "Which properties make a cryptographic hash function suitable here?",
                ("Deterministic - the same input always gives the same digest", true),
                ("One-way - the input cannot be recovered from the digest", true),
                ("Fixed-size output regardless of input size", true),
                ("Reversible with the correct key", false))));

    private static Module Consensus() => NewModule(
        "Blocks and Consensus",
        "How a decentralised network agrees on one history: proof of work, proof of stake, forks and finality.",
        75,
        aiAvatarEnabled: true,
        NewLesson(
            "Proof of work",
            """
            Consensus is the mechanism by which participants who cannot trust each other converge on a single
            ordered history.

            Proof of work makes extending the chain deliberately expensive. Miners assemble candidate blocks
            and repeatedly hash the header with a varying number called a nonce, searching for a digest below a
            target value. Because the hash is unpredictable, the only strategy is to try enormous numbers of
            nonces. Finding one is hard; checking it is instant, which is precisely the asymmetry the design
            needs.

            The network adjusts the target periodically so that blocks arrive at a roughly constant rate no
            matter how much hardware joins. Bitcoin retargets every 2016 blocks to hold a ten-minute average.

            When two miners find a block at nearly the same moment the chain forks temporarily. Nodes follow
            the rule of accumulated work, so as soon as one branch is extended it wins and the other is
            orphaned. Transactions in the abandoned branch return to the pool. This is why confirmations
            matter: a transaction is not final but exponentially harder to reverse as blocks pile on top, and
            waiting several confirmations is a probabilistic judgement rather than a guarantee.

            An attacker with more than half the network's hash power can outpace the honest chain. That lets
            them exclude transactions and reverse their own recent spends. It does not let them steal from
            addresses they lack keys for, or invent coins from nothing - those are enforced by validation
            rules every node checks independently.

            The cost of this security is energy, paid continuously and by design.
            """),
        NewLesson(
            "Proof of stake and forks",
            """
            Proof of stake replaces expensive computation with an economic bond. Validators lock up capital,
            and the protocol selects among them to propose and attest to blocks, roughly in proportion to stake.

            The security argument shifts from "attacking costs electricity" to "attacking costs your deposit".
            Misbehaviour such as signing two conflicting blocks is detectable by anyone, and the protocol
            destroys part of the offender's stake - slashing. Energy consumption falls by orders of magnitude,
            since no brute-force search takes place.

            Proof of stake also enables explicit finality. Rather than reversal merely becoming improbable,
            checkpoints can be declared final, such that reverting them would require destroying an
            economically prohibitive share of the total stake. The main criticism is that influence tracks
            capital, which may concentrate over time - especially where custodial staking services aggregate
            many small holders.

            Protocol changes come as forks. A soft fork tightens the rules: blocks valid under the new rules
            remain valid under the old, so nodes that do not upgrade continue to follow the chain. A hard fork
            loosens or changes rules incompatibly, so every participant must upgrade. Nodes that refuse
            continue on their own chain, which is how a contested hard fork produces two coins with a shared
            history.

            Consensus rules are ultimately social. The software enforces them, but which software the
            participants choose to run is a decision made by people.
            """),
        NewExercise(
            "Compare consensus for a use case",
            """
            A consortium of six hospitals wants a shared, tamper-evident audit log of who accessed which
            patient record. All six are known, contractually bound, and regulated.

            1. Evaluate permissionless proof of work for this case.
            2. Evaluate proof of stake.
            3. Evaluate a permissioned chain among the six.
            4. Evaluate an append-only database with signed entries and third-party notarisation.
            5. Recommend one, with the trade-off you are accepting.
            """,
            """
            Proof of work is a poor fit: the participants are known and contractually accountable, so paying
            for permissionless Sybil resistance buys nothing while adding latency, cost and public exposure of
            metadata. Proof of stake reduces the energy cost but keeps machinery aimed at a problem this
            consortium does not have. A permissioned chain is defensible, mainly for the shared-write and
            tamper-evidence properties. The signed append-only log is simplest and often sufficient. The honest
            recommendation is the last, or a permissioned chain if genuinely no member can be trusted to host -
            accepting operational complexity in exchange for removing that single host.
            """),
        NewQuiz(
            "Consensus check",
            Choice(
                "What are proof-of-work miners actually searching for?",
                ("A nonce that makes the block header hash fall below a target value", true),
                ("A prime factorisation of the previous hash", false),
                ("The private key of the recipient", false),
                ("An unused transaction identifier", false)),
            Choice(
                "An attacker controlling a majority of hash power can:",
                ("Reorder or censor transactions and reverse their own recent spends", true),
                ("Steal coins from any address", false),
                ("Create coins beyond the protocol's issuance rules", false),
                ("Read private keys from the chain", false)),
            TrueFalse(
                "A soft fork keeps blocks valid under both old and new rules, so unupgraded nodes still follow "
                    + "the chain.",
                true),
            Choice(
                "In proof of stake, slashing refers to:",
                ("Destroying part of a validator's stake as a penalty for provable misbehaviour", true),
                ("Reducing the block reward over time", false),
                ("Cutting the block interval in half", false),
                ("Removing old transactions to save space", false)),
            Multiple(
                "Which are genuine trade-offs of proof of stake relative to proof of work?",
                ("Far lower energy consumption", true),
                ("Influence tracks capital, which may concentrate", true),
                ("It can offer explicit finality rather than only probabilistic settlement", true),
                ("It removes the need for consensus altogether", false))));

    private static Module BitcoinAndEthereum() => NewModule(
        "Bitcoin, Ethereum and the Transaction Lifecycle",
        "Two different ledger models, what gas pays for, and what happens between signing and confirmation.",
        75,
        aiAvatarEnabled: false,
        NewLesson(
            "UTXO versus accounts",
            """
            Bitcoin has no balances. It tracks unspent transaction outputs - discrete chunks of value, each
            locked to a condition, usually "whoever can sign for this public key". Spending consumes whole
            outputs as inputs and creates new ones, so paying 3 from a 5 output produces a 3 output to the
            recipient and a 2 change output back to you. Your wallet balance is simply the sum of the outputs
            you can unlock.

            This model parallelises well, since independent outputs can be validated independently, and it
            improves privacy through fresh change addresses. It is awkward for anything stateful.

            Ethereum instead keeps accounts with balances, like a bank ledger. Externally owned accounts are
            controlled by a private key. Contract accounts hold code and persistent storage, and act only when
            called. The account model is far more natural for programmable state, which is why smart contracts
            live here.

            The account model creates an ordering problem the UTXO model does not have: two transactions from
            the same account could conflict. Ethereum solves it with a per-account nonce, a counter that must
            increase by exactly one each transaction. Transactions are executed strictly in nonce order, which
            is why a transaction stuck with too low a fee blocks every later one from that account until it
            confirms or is replaced.

            Gas measures computational work. Every operation has a gas cost, and the sender sets both a gas
            limit and a price per unit. Fees exist to price a scarce shared resource and to make infinite loops
            self-limiting: a transaction that runs out of gas reverts all its state changes, but the gas
            already consumed is still paid. That asymmetry is the point - the network did the work.
            """),
        NewLesson(
            "From signing to confirmation",
            """
            A transaction passes through a predictable sequence, and it can fail at every step.

            First it is constructed and signed locally. The private key never leaves the wallet; what is
            broadcast is the transaction plus a signature over it.

            It is then broadcast to a peer, which validates it - signature correct, nonce sensible, balance
            sufficient, fee above the node's minimum - and gossips it onward. Failing validation means it never
            propagates at all.

            Valid transactions wait in the mempool, an unordered pool of pending work. Block producers select
            from it, and because block space is scarce they generally take the highest-paying transactions
            first. A transaction with too low a fee can sit for a long time, or be dropped when the mempool
            fills.

            Inclusion in a block is not the end. Until enough subsequent blocks accumulate, a reorganisation
            could still displace it. Exchanges wait a set number of confirmations before crediting a deposit
            for exactly this reason.

            Public mempools have an uncomfortable consequence: pending transactions are visible before they
            execute. Anyone can see a large pending trade and pay more to be ordered ahead of it, profiting at
            the original sender's expense. This value, extractable by whoever controls ordering, is known as
            MEV, and it is an ordinary feature of transparent ordering rather than a bug in any one
            application.

            A decentralised application ties this together: a frontend, a wallet holding the keys, an RPC node
            for network access, and contracts holding the on-chain logic. Note that the frontend and the RPC
            endpoint are usually conventionally hosted - "decentralised" describes the settlement layer, not
            necessarily the whole stack.
            """),
        NewExercise(
            "Follow a transfer end to end",
            """
            A user sends 50 tokens from an Ethereum account.

            1. List each stage from construction to final confirmation.
            2. At each stage, name one concrete way it can fail.
            3. The user submits a second transfer before the first confirms, using a higher fee. Explain what
               happens and why, referring to the nonce.
            4. Explain why a transaction that runs out of gas still costs the sender money.
            5. Identify where in the lifecycle front-running becomes possible.
            """,
            """
            Construction and signing (wrong recipient, or key unavailable); broadcast and validation (bad
            signature, insufficient balance, fee below the node minimum); mempool (stuck indefinitely at too
            low a fee); inclusion (reverts on-chain, consuming gas); confirmation (displaced by a reorg). If the
            second transfer reuses the same nonce it replaces the first when the fee bump is sufficient; if it
            uses the next nonce it cannot execute until the first does. Out-of-gas reverts state but the work
            was performed, so the gas is still charged. Front-running becomes possible the moment the
            transaction is visible in the public mempool, before ordering is fixed.
            """),
        NewQuiz(
            "Transactions check",
            Choice(
                "In the UTXO model, a wallet's balance is:",
                ("The sum of the unspent outputs it can unlock", true),
                ("A single number stored in the account record", false),
                ("Computed by the miner at each block", false),
                ("The difference between deposits and withdrawals in the header", false)),
            Choice(
                "The per-account nonce on Ethereum exists to:",
                ("Force transactions from one account to execute in a strict, non-repeatable order", true),
                ("Randomise the transaction hash", false),
                ("Set the gas price", false),
                ("Identify the recipient", false)),
            TrueFalse(
                "A transaction that runs out of gas reverts its state changes, but the gas consumed is still "
                    + "paid.",
                true),
            Choice(
                "Where does a valid but not yet included transaction wait?",
                ("In the mempool", true),
                ("In the Merkle tree", false),
                ("In the block header", false),
                ("In the wallet's cold storage", false)),
            Multiple(
                "Which parts make up a typical decentralised application?",
                ("A frontend that users interact with", true),
                ("A wallet holding the user's private keys", true),
                ("An RPC node providing network access", true),
                ("A central database holding user balances", false))));

    private static Module SmartContracts() => NewModule(
        "Smart Contracts with Solidity",
        "Programs that run on a shared machine: storage, gas, events, immutability and upgrade patterns.",
        90,
        aiAvatarEnabled: false,
        NewLesson(
            "The EVM and contract structure",
            """
            A smart contract is a program deployed to an address, with persistent storage, that executes when
            called. The Ethereum Virtual Machine runs it identically on every node, which is what makes the
            result verifiable - and which forces strict determinism. A contract cannot read a random number,
            call an HTTP API, or ask the time beyond the block timestamp, because every node must reach the
            same answer.

            A Solidity contract declares state variables that persist in storage, and functions that read or
            modify them. Visibility is explicit: public, external, internal or private. Functions marked view
            or pure make no state changes and cost nothing when called off-chain.

            Where data lives is a first-order concern, not a detail. Storage is persistent and by far the most
            expensive resource - writing a fresh storage slot costs thousands of gas. Memory exists only for
            the duration of a call and is cheap. Calldata is the read-only argument area and is cheapest of
            all. Reading a storage variable inside a loop instead of caching it in memory is one of the most
            common avoidable costs in real contracts.

            Guard preconditions with require, which reverts the transaction and refunds unused gas when the
            condition fails. Reverting is the normal, correct way for a contract to refuse.

            Events are the contract's log. They are far cheaper than storage and are how off-chain
            applications observe what happened, since contracts cannot push notifications. Anything a user
            interface needs to display should be emitted as an event.
            """),
        NewLesson(
            "Immutability and upgrade patterns",
            """
            Deployed bytecode cannot be modified. There is no patch, no hotfix and no rollback. A bug is
            permanent unless the contract was designed in advance to accommodate change, and the history of
            this space is largely a history of that lesson being learned expensively.

            The proxy pattern is the usual answer. Users interact with a proxy that holds all the storage and
            forwards calls to a separate logic contract. Upgrading means pointing the proxy at new logic while
            the address and state stay put. This works, but it introduces real hazards: the storage layout of
            the new logic must remain compatible with the old, since variables are addressed by slot position
            rather than name, and inserting a variable in the middle silently reinterprets existing data. The
            proxy's initialiser must also be protected, since a constructor does not run in the proxy's
            context.

            It also raises an honest question. Whoever can upgrade the logic can change the rules, so an
            upgradeable contract is only as trustworthy as the key controlling it. Timelocks and multisig
            control are the usual mitigations - they do not remove the trust assumption, they make its exercise
            visible and slow.

            Some contracts deliberately forgo upgradeability, accepting permanent bugs in exchange for a
            credible promise that the rules cannot change. That is a legitimate design choice, not an
            oversight.

            Whichever route you take, the practical discipline is the same: extensive tests, an external audit,
            a public testnet deployment, and a staged rollout with value caps. Audits reduce risk; they do not
            eliminate it, and several audited contracts have been drained.
            """),
        NewExercise(
            "Find the bug",
            """
            Review this withdrawal function:

            function withdraw(uint amount) public {
                (bool ok, ) = msg.sender.call{value: amount}("");
                require(ok, "transfer failed");
                balances[msg.sender] -= amount;
            }

            1. Identify the vulnerability by name.
            2. Explain the exact sequence an attacking contract would use.
            3. Note the second, independent bug in this function.
            4. Rewrite it correctly and name the pattern you applied.
            """,
            """
            This is reentrancy. The external call happens before the balance is reduced, so a malicious
            recipient's fallback function can call withdraw again while the stored balance is still the
            original value, repeating until the contract is drained. Independently, there is no check that the
            caller's balance is at least the amount requested. The fix applies checks-effects-interactions:
            require a sufficient balance, deduct it, and only then make the external call - optionally with a
            reentrancy guard as defence in depth.
            """),
        NewQuiz(
            "Smart contracts check",
            Choice(
                "Can a deployed contract's code be edited in place?",
                ("No - upgrades require a pattern such as a proxy delegating to new logic", true),
                ("Yes, by the original deployer at any time", false),
                ("Yes, by majority vote of token holders", false),
                ("Yes, but only within 24 hours of deployment", false)),
            Choice(
                "Which data location is persistent and most expensive?",
                ("Storage", true),
                ("Memory", false),
                ("Calldata", false),
                ("The stack", false)),
            TrueFalse(
                "A smart contract can call an external HTTP API directly to fetch current prices.",
                false),
            Choice(
                "The checks-effects-interactions pattern prevents:",
                ("Reentrancy, by updating state before making external calls", true),
                ("Integer overflow", false),
                ("Front-running", false),
                ("Storage collisions in proxies", false)),
            Multiple(
                "Which are real hazards of the upgradeable proxy pattern?",
                ("Storage layout must stay compatible, since slots are positional", true),
                ("Whoever controls the upgrade key can change the rules", true),
                ("The initialiser must be protected, as constructors do not run in the proxy context", true),
                ("The proxy address changes on every upgrade", false))));

    private static Module TokensAndRisk() => NewModule(
        "Tokens, DeFi and Security Pitfalls",
        "Token standards, automated market makers, the failure modes that keep recurring, and honest limits.",
        80,
        aiAvatarEnabled: false,
        NewLesson(
            "Token standards and automated market makers",
            """
            A token is not a separate coin; it is a balance table inside a contract. ERC-20 standardises the
            interface for fungible tokens - transfer, approve, balanceOf and the rest - so wallets and
            exchanges can integrate any compliant token without bespoke code. ERC-721 does the same for
            non-fungible tokens, where each identifier is unique and ownership is tracked per token.

            The approve-then-transferFrom flow deserves attention because it is where users lose money. Rather
            than sending tokens to a contract, you authorise it to move them on your behalf. Interfaces
            commonly request unlimited approval for convenience, which leaves a standing permission to drain
            that token balance if the approved contract is ever compromised. Reviewing and revoking approvals
            is basic hygiene.

            Automated market makers replaced order books for on-chain trading. A pool holds reserves of two
            assets and prices them by a formula, classically the constant product x * y = k. Anyone can trade
            against the pool; the price moves along the curve as reserves shift. Liquidity providers deposit
            both assets and earn a share of fees.

            Two consequences follow directly. Large trades move the price against themselves - slippage - which
            is why trades specify a minimum acceptable output. And liquidity providers face impermanent loss:
            when the external price of one asset moves, arbitrageurs rebalance the pool, leaving the provider
            with more of the asset that fell and less of the one that rose. Compared with simply holding both
            assets, that is a loss, offset only if fee income exceeds it. The name is misleading, since the
            loss becomes permanent on withdrawal.
            """),
        NewLesson(
            "Recurring failure modes and honest limits",
            """
            The same vulnerabilities keep appearing.

            Reentrancy remains the archetype: an external call hands control to untrusted code before state is
            settled. The 2016 DAO incident drained around a third of the ether then in circulation and led to
            the hard fork that split Ethereum Classic from Ethereum.

            Integer overflow silently wrapped arithmetic before Solidity 0.8 made it revert by default. Old
            code and unchecked blocks remain exposed.

            Access control failures - a function that should have been restricted to an owner but was left
            callable by anyone - are unglamorous and have caused some of the largest losses in the space.

            Oracle manipulation is the modern favourite. A contract that reads a price from a single on-chain
            pool can be attacked by moving that pool's price with a flash loan, borrowing millions with no
            collateral within one transaction, exploiting the distorted price, and repaying in the same
            transaction. The defence is to use time-weighted averages or several independent oracle sources.

            Beyond code, governance and key management are frequently the real weakness: an upgradeable
            contract behind a single key, or a bridge holding pooled assets, concentrates risk regardless of
            how clean the Solidity is. Bridges have been among the largest targets precisely for this reason.

            The honest summary is that blockchains buy one specific property - shared state that no single
            party controls - and charge heavily for it in throughput, cost, latency and irreversibility. When
            you can identify the participants and one of them can be trusted to run the system, a database with
            signed audit records is faster, cheaper, private, and correctable when something goes wrong. Being
            able to say that clearly is a mark of understanding the technology, not of doubting it.
            """),
        NewExercise(
            "Blockchain or database?",
            """
            For each scenario, decide whether a blockchain is justified, and state the single property that
            drives your answer:

            1. A national land registry operated by a government agency.
            2. Settlement between five banks that do not trust one another's ledgers but are regulated.
            3. A loyalty points scheme for one retail chain.
            4. Provenance tracking for luxury goods across many independent suppliers.
            5. Internal audit logs for a single company's IT department.

            For each case where you reject a blockchain, name what you would build instead.
            """,
            """
            1 does not need one - the agency is already the authority, and its problems are data quality and
            corruption, which a ledger does not fix. 2 is the strongest case: mutually distrusting parties
            needing shared settlement, though a permissioned chain suffices. 3 is a database; the issuer
            controls the points and can reverse errors. 4 is genuinely arguable across many independent
            parties, but the weak link is the physical-to-digital binding, which no chain can secure. 5 is an
            append-only signed log. The recurring test is whether the participants are mutually distrusting and
            whether removing a central operator is truly required.
            """),
        NewQuiz(
            "Tokens and risk check",
            Choice(
                "An ERC-20 token balance is:",
                ("An entry in a mapping inside the token contract", true),
                ("A separate blockchain per token", false),
                ("A file stored on the user's device", false),
                ("A UTXO locked to the holder's key", false)),
            Choice(
                "Impermanent loss affects:",
                ("Liquidity providers, when the relative price of the pooled assets changes", true),
                ("Traders, when a transaction reverts", false),
                ("Validators, when they are slashed", false),
                ("Token holders, when supply inflates", false)),
            TrueFalse(
                "A completed security audit guarantees a smart contract is safe to hold significant value.",
                false),
            Choice(
                "A flash loan enables oracle manipulation because it:",
                ("Supplies large uncollateralised capital within a single transaction to distort a price", true),
                ("Permanently transfers ownership of a pool", false),
                ("Disables the contract's require statements", false),
                ("Rewrites the block's Merkle root", false)),
            Multiple(
                "Which are recurring smart contract vulnerability classes?",
                ("Reentrancy", true),
                ("Missing or incorrect access control", true),
                ("Oracle manipulation via a single on-chain price source", true),
                ("Storing data in memory rather than storage", false))));

    private static Module FinalAssessment() => NewModule(
        "Final Assessment",
        "A timed exam covering primitives, consensus, transactions, contracts and security.",
        45,
        aiAvatarEnabled: false,
        NewExam(
            "Blockchain Fundamentals - Final Exam",
            Choice(
                "Editing a transaction in block 5 of a 10-block chain invalidates:",
                ("Blocks 5 through 10, because each header commits to the previous hash", true),
                ("Only block 5", false),
                ("Only blocks 1 through 5", false),
                ("Nothing, provided the Merkle root is recomputed", false)).Worth(2),
            TrueFalse(
                "Public blockchain data is hashed for tamper evidence, not encrypted for confidentiality.",
                true).Worth(2),
            Choice(
                "An entity controlling a majority of proof-of-work hash power can:",
                ("Censor transactions and reverse its own recent spends", true),
                ("Spend from any address on the network", false),
                ("Mint coins outside the issuance schedule", false),
                ("Decrypt other users' private keys", false)).Worth(2),
            Choice(
                "The primary purpose of gas is to:",
                ("Price scarce computation and make unbounded execution self-limiting", true),
                ("Pay dividends to token holders", false),
                ("Encrypt the transaction payload", false),
                ("Reserve a slot in the Merkle tree", false)).Worth(2),
            Multiple(
                "Which are true of the account model compared with UTXO?",
                ("It stores balances directly rather than discrete unspent outputs", true),
                ("It uses a per-account nonce to fix transaction ordering", true),
                ("It is more natural for stateful smart contracts", true),
                ("It removes the need for digital signatures", false)).Worth(3),
            Choice(
                "Checks-effects-interactions defends against reentrancy by:",
                ("Updating state before making any external call", true),
                ("Limiting the gas forwarded to zero", false),
                ("Encrypting the contract's storage", false),
                ("Requiring a multisig for every withdrawal", false)).Worth(2),
            Choice(
                "The main storage hazard in an upgradeable proxy is:",
                ("New logic must preserve the existing slot layout, since slots are positional", true),
                ("Storage is erased on every upgrade", false),
                ("The proxy address changes each time", false),
                ("Events stop being emitted", false)).Worth(2),
            Multiple(
                "Which conditions genuinely favour a blockchain over a conventional database?",
                ("The participants are mutually distrusting", true),
                ("No single party may control the shared state", true),
                ("Irreversibility of settled records is a requirement rather than a risk", true),
                ("The application needs the highest possible transaction throughput", false)).Worth(3)));
}
