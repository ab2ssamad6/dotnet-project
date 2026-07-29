using Lms.Domain.Entities;
using Lms.Domain.Enums;

using static Lms.Infrastructure.Persistence.Seed.DemoData.DemoContent;

namespace Lms.Infrastructure.Persistence.Seed.DemoData;

public static class MachineLearningFoundationsCourse
{
    public const string Title = "Machine Learning Foundations";

    public static Training Create(Category category, Trainer trainer) => NewTraining(
        Title,
        "A practical, mathematics-light introduction to machine learning. You will learn how models actually "
            + "learn from data, how to prepare a dataset without fooling yourself, how to tell a good model from "
            + "a lucky one, and when machine learning is the wrong tool for the job.",
        DifficultyLevel.Beginner,
        category,
        trainer,
        WhatIsMl(),
        DataPreparation(),
        SupervisedLearning(),
        ModelEvaluation(),
        UnsupervisedLearning(),
        FinalAssessment());

    private static Module WhatIsMl() => NewModule(
        "What is Machine Learning?",
        "How machines learn from data, how supervised and unsupervised learning differ, and where the "
            + "approach breaks down.",
        45,
        aiAvatarEnabled: true,
        NewLesson(
            "Learning from data instead of rules",
            """
            Traditional software encodes rules a person worked out in advance. Machine learning inverts that:
            you supply examples of the input and the desired output, and an algorithm searches for a rule that
            reproduces them.

            That inversion is worth taking seriously. Nobody can write down the rule that separates a photo of
            a cat from a photo of a dog, but anyone can collect ten thousand labelled photos. Machine learning
            is the right tool exactly when examples are easy to gather and rules are hard to state.

            Three broad families cover almost everything you will meet:

            - Supervised learning. Every training example carries a correct answer, called a label. The model
              learns a mapping from inputs to labels. Spam detection, price prediction and medical triage all
              live here.
            - Unsupervised learning. There are no labels. The algorithm looks for structure on its own -
              grouping similar customers, compressing many measurements into a few, flagging the unusual.
            - Reinforcement learning. An agent acts in an environment and receives rewards. It learns a policy
              by trial and error. Game playing and robot control live here.

            The vocabulary is small. A feature is one input measurement. A label is the answer you want to
            predict. An example is one row: its features plus, in supervised learning, its label. A model is
            the object produced by training. Training is the search for a good model; inference is using it.
            """),
        NewLesson(
            "When not to use machine learning",
            """
            A model that is technically correct can still be the wrong thing to build. Reach for ordinary code
            instead when:

            - The rule is already known and stable. Tax brackets do not need to be learned; they are published.
              A learned approximation would be less accurate, harder to audit and would drift.
            - You cannot get representative data. A model trained on last year's customers describes last
              year's customers. If the world moved, so did the ground truth.
            - Errors are unacceptable and unexplainable. Every model is wrong sometimes. If you cannot say why
              it refused a loan, you may not be permitted to deploy it at all.
            - The dataset is tiny. A handful of examples cannot pin down a general rule, and the model will
              memorise instead of generalise.

            Two failures cause most disappointment in practice. The first is a mismatched objective: optimising
            click-through when the business needs retention produces a model that succeeds at the wrong task.
            The second is bias inherited from data. A model trained on historical hiring decisions learns
            historical hiring prejudice and applies it faster and more consistently than any human did.

            Machine learning is a tool for prediction under uncertainty. It is not a substitute for
            understanding the problem.
            """),
        NewExercise(
            "Classify the problem type",
            """
            For each scenario, decide whether it is supervised, unsupervised, reinforcement learning, or not a
            machine learning problem at all, and justify the choice in one sentence:

            1. Predicting tomorrow's electricity demand from ten years of hourly readings.
            2. Grouping ten million users into segments nobody has defined in advance.
            3. Converting a temperature from Celsius to Fahrenheit.
            4. Teaching a warehouse robot to pick the shortest route through changing aisles.
            5. Flagging credit card transactions as fraudulent, given a history of confirmed fraud.
            6. Deciding whether a customer is eligible for a discount defined by a published policy.
            """,
            """
            1 and 5 are supervised - both have historical labels. 2 is unsupervised, with no target to predict.
            4 is reinforcement learning: the reward arrives after a sequence of actions. 3 and 6 are not machine
            learning; both are exact rules that should simply be implemented, and learning them would be less
            accurate and harder to audit.
            """),
        NewQuiz(
            "Foundations check",
            Choice(
                "What distinguishes supervised from unsupervised learning?",
                ("Supervised training examples carry a correct answer, or label", true),
                ("Supervised learning requires more data", false),
                ("Supervised learning always uses neural networks", false),
                ("Unsupervised learning runs without a computer", false)),
            Choice(
                "In a table of house sales used to predict price, the price column is:",
                ("A feature", false),
                ("The label", true),
                ("A hyperparameter", false),
                ("An outlier", false)),
            TrueFalse(
                "Machine learning is the right approach when the rule is already known, published and stable.",
                false),
            Choice(
                "A model trained on historical hiring decisions tends to:",
                ("Reproduce the biases present in those decisions", true),
                ("Remove human bias automatically", false),
                ("Refuse to train on categorical data", false),
                ("Converge to a fair outcome given enough data", false)),
            Multiple(
                "Which are genuine reasons to avoid machine learning for a problem?",
                ("The available data is not representative of where the model will be used", true),
                ("Decisions must be explainable and errors are unacceptable", true),
                ("The exact rule is already known", true),
                ("The team already knows Python", false))));

    private static Module DataPreparation() => NewModule(
        "Data Preparation and Feature Engineering",
        "Splitting data honestly, avoiding leakage, and turning raw columns into features a model can use.",
        75,
        aiAvatarEnabled: true,
        NewLesson(
            "Splitting data and the leakage trap",
            """
            A model's score on the data it trained on is meaningless - it has already seen the answers. To
            estimate real performance you must hold data back.

            The standard split is three ways:

            - Training set, typically 60-80%. The model fits this.
            - Validation set. You use it to choose between models and to tune hyperparameters.
            - Test set. Touched once, at the very end, to report the number you will quote.

            The validation set exists because tuning is itself a form of fitting. If you pick the settings that
            score best on the test set, you have fitted to the test set and its estimate is no longer honest.

            Data leakage is when information that would not be available at prediction time slips into
            training. It produces spectacular validation scores and a model that collapses in production. The
            classic case is scaling: if you compute the mean and standard deviation over the whole dataset and
            then split, the training set has absorbed information about the test rows. Fit every transformation
            on the training set alone, then apply it unchanged to validation and test.

            Leakage also hides in time. If your data has an ordering - transactions, sensor readings, anything
            forecast-like - a random split lets the model train on the future and predict the past. Split by
            time instead, and be suspicious of any feature computed from a window that spans the split.
            """),
        NewLesson(
            "Making raw columns usable",
            """
            Most real datasets are not numeric matrices, and most of the work is turning them into one.

            Missing values must be handled explicitly, because most algorithms cannot consume them. Dropping
            rows is safe only when they are few and missing at random. Filling with the column mean or median
            keeps the row but flattens variance; the median is more robust to outliers. Often the fact that a
            value is missing is itself informative, so add a boolean "was missing" column alongside the fill.

            Categorical columns need encoding. One-hot encoding creates one binary column per category and
            makes no claim about ordering, which is what you want for colours or cities. Ordinal encoding maps
            categories to integers and should be used only when the order is real - small, medium, large. Using
            ordinal encoding on unordered categories quietly tells the model that city 3 sits between city 2
            and city 4.

            Numeric scaling matters for any algorithm that measures distance or uses gradient descent -
            k-nearest neighbours, support vector machines, neural networks. Standardisation rescales a column
            to zero mean and unit variance; min-max scaling squeezes it into a fixed range. Tree-based models
            split on thresholds one column at a time and are indifferent to scale.

            Finally, look at class balance. If 99% of transactions are legitimate, a model that always predicts
            "legitimate" is 99% accurate and completely useless. Resampling, class weights and a metric other
            than accuracy are the usual responses.
            """),
        NewExercise(
            "Build a preprocessing plan",
            """
            You are given a customer churn dataset with these columns: age (numeric, 4% missing), country
            (categorical, 40 values), subscription tier (small / medium / large), monthly spend (numeric, heavy
            right skew), signup date, and churned (boolean target, 8% true).

            Write the preprocessing steps in the order you would apply them, and for each state:

            1. What you do and why.
            2. Whether it is fitted on training data only.

            Then name the leakage risk in this dataset and how you would split to avoid it.
            """,
            """
            Split first, by signup date rather than randomly. Then fit imputation and scaling on the training
            portion alone. Age gets median imputation plus a "was missing" indicator; country gets one-hot
            encoding or grouping of rare values; subscription tier gets ordinal encoding because the order is
            genuine; monthly spend gets a log transform before scaling. The 8% positive rate means accuracy is
            the wrong metric - class weights plus precision and recall are appropriate.
            """),
        NewQuiz(
            "Data preparation check",
            Choice(
                "Why must a scaler be fitted on the training set only?",
                ("Otherwise the training data absorbs information about the held-out rows - leakage", true),
                ("Because fitting on all data is computationally expensive", false),
                ("Because scalers cannot process more than one split", false),
                ("It makes no difference as long as the split is random", false)),
            Choice(
                "Which encoding suits an unordered categorical column such as country?",
                ("One-hot encoding", true),
                ("Ordinal encoding", false),
                ("Standardisation", false),
                ("Log transformation", false)),
            TrueFalse(
                "For time-ordered data a random train/test split lets the model train on the future.",
                true),
            Choice(
                "On a dataset where 99% of rows are the negative class, accuracy is a poor metric because:",
                ("A model that always predicts the majority class scores 99% while being useless", true),
                ("Accuracy cannot be computed on imbalanced data", false),
                ("Accuracy always underestimates performance", false),
                ("It only applies to regression", false)),
            Multiple(
                "Which are reasonable ways to handle missing numeric values?",
                ("Impute the median and add a boolean indicating the value was missing", true),
                ("Drop the rows when they are few and missing at random", true),
                ("Replace them with zero without checking what zero means in that column", false),
                ("Impute the mean, accepting that it flattens variance", true))));

    private static Module SupervisedLearning() => NewModule(
        "Supervised Learning in Practice",
        "Regression and classification, the algorithms that cover most real problems, and how they overfit.",
        90,
        aiAvatarEnabled: false,
        NewLesson(
            "Regression and classification",
            """
            Supervised learning splits by the kind of label. Predicting a number is regression; predicting a
            category is classification.

            Linear regression fits a straight line - or a plane in higher dimensions - by choosing coefficients
            that minimise the squared error between predictions and truth. Its virtue is interpretability: each
            coefficient states how much the prediction moves per unit of that feature, holding the others
            fixed. Its limitation is that it can only express straight-line relationships unless you engineer
            curved features yourself.

            Logistic regression, despite the name, classifies. It computes the same weighted sum and then
            passes it through a sigmoid that squashes any number into the range 0 to 1, read as a probability.
            You then choose a threshold, conventionally 0.5, above which you predict the positive class. That
            threshold is a business decision, not a mathematical one: lowering it catches more positives at the
            cost of more false alarms.

            Decision trees ask a sequence of yes/no questions, choosing at each step the split that best
            separates the classes. They handle non-linear relationships and mixed data types without
            preprocessing, and a shallow tree can be read aloud. Left unconstrained, though, a tree will keep
            splitting until every leaf is pure - memorising the training set exactly.

            Random forests fix that by training many trees, each on a random subsample of rows and features,
            and averaging their votes. Individual trees overfit in different directions and the errors largely
            cancel. Forests are the reliable default for tabular data: strong out of the box, hard to break.
            """),
        NewLesson(
            "Overfitting and the bias-variance tradeoff",
            """
            Overfitting is a model learning the noise in the training data as if it were signal. The symptom is
            unmistakable: excellent training performance, poor performance on held-out data.

            Underfitting is the opposite - the model is too simple to capture the real pattern, and performs
            poorly on both sets.

            These are the two ends of the bias-variance tradeoff. High bias means the model makes strong,
            possibly wrong assumptions and misses real structure; that is underfitting. High variance means the
            model is so flexible that it swings with the particular sample it saw; that is overfitting. Total
            error is the sum of the two, and reducing one usually raises the other, so the goal is the balance
            point rather than the elimination of either.

            The practical toolkit is short:

            - More training data reduces variance and almost never hurts.
            - Simplify the model: fewer features, a shallower tree, a lower polynomial degree.
            - Regularisation adds a penalty on large coefficients to the loss. L2, or ridge, shrinks all
              coefficients smoothly. L1, or lasso, drives some to exactly zero and therefore performs feature
              selection.
            - Early stopping halts iterative training when validation error stops improving, even if training
              error would keep falling.

            Diagnose before you treat. If training and validation scores are both poor, the model is
            underfitting and regularising it further will make things worse.
            """),
        NewExercise(
            "Diagnose three models",
            """
            Each row reports accuracy on the training and validation sets. Diagnose each model and prescribe a
            specific fix:

            1. Training 0.99, validation 0.71
            2. Training 0.68, validation 0.67
            3. Training 0.91, validation 0.89

            Then explain why applying the fix for case 1 to case 2 would make case 2 worse.
            """,
            """
            Case 1 overfits - a large gap with high training accuracy. Reduce complexity, add regularisation,
            or gather more data. Case 2 underfits: both scores are low and close, so the model is too simple;
            add features or capacity. Case 3 is healthy. Applying case 1's remedy to case 2 would constrain an
            already too-simple model, raising bias and lowering both scores further.
            """),
        NewQuiz(
            "Supervised learning check",
            Choice(
                "Predicting how many minutes a delivery will take is:",
                ("A regression problem, because the label is a number", true),
                ("A classification problem", false),
                ("An unsupervised problem", false),
                ("A reinforcement learning problem", false)),
            Choice(
                "What does the sigmoid in logistic regression do?",
                ("Squashes any real number into the range 0 to 1, read as a probability", true),
                ("Removes outliers from the input", false),
                ("Selects the most important feature", false),
                ("Guarantees a linear decision boundary in the original features", false)),
            TrueFalse(
                "Training accuracy far above validation accuracy is the classic symptom of overfitting.",
                true),
            Choice(
                "Which regularisation can drive coefficients to exactly zero, performing feature selection?",
                ("L1, or lasso", true),
                ("L2, or ridge", false),
                ("Standardisation", false),
                ("Early stopping", false)),
            Multiple(
                "Why does a random forest usually outperform a single deep decision tree?",
                ("Individual trees overfit in different directions and their errors partly cancel", true),
                ("Each tree sees a random subsample of rows and features", true),
                ("Averaging many trees reduces variance", true),
                ("A forest is guaranteed to have zero bias", false))));

    private static Module ModelEvaluation() => NewModule(
        "Model Evaluation",
        "Choosing the metric that matches the cost of being wrong, and measuring it without fooling yourself.",
        60,
        aiAvatarEnabled: false,
        NewLesson(
            "Beyond accuracy",
            """
            Every classification result falls into one of four cells. A true positive is correctly flagged, a
            true negative correctly ignored. A false positive is a false alarm; a false negative is a miss.
            Arranged as a table, these four counts form the confusion matrix, and every classification metric
            is a ratio drawn from it.

            Accuracy is the share of all predictions that were right. It is only meaningful when the classes
            are roughly balanced and the two error types cost about the same.

            Precision asks: of everything the model flagged, what share really was positive? It is the metric
            you care about when false alarms are expensive - marking legitimate email as spam, say.

            Recall asks: of everything that really was positive, what share did the model catch? It is the
            metric you care about when misses are expensive - screening for a treatable disease.

            Precision and recall trade off against each other through the decision threshold. Lower it and you
            catch more positives, at the cost of more false alarms. The F1 score is the harmonic mean of the
            two, useful as a single number when both matter and you cannot express their relative cost.

            ROC-AUC summarises performance across every possible threshold. An AUC of 1.0 is perfect; 0.5 is
            indistinguishable from guessing. Because it is threshold-free it is good for comparing models, but
            it can look flattering on very imbalanced data, where precision-recall curves are more honest.

            Choose the metric before you train. Choosing it afterwards, once you have seen the scores, is how
            teams talk themselves into shipping a bad model.
            """),
        NewLesson(
            "Cross-validation and honest estimates",
            """
            A single train/validation split gives one estimate from one arbitrary partition. On a small dataset
            that estimate is noisy: a slightly different split can move the score by several points, and you
            cannot tell improvement from luck.

            k-fold cross-validation splits the data into k equal folds, then trains k times, each time holding
            out a different fold for validation. Averaging the k scores gives a more stable estimate, and their
            spread tells you how much to trust it - a mean of 0.85 with a standard deviation of 0.02 is a very
            different situation from the same mean with a spread of 0.15.

            Two variants matter in practice. Stratified k-fold preserves the class proportions in every fold,
            which is essential on imbalanced data. Time-series cross-validation always trains on earlier data
            and validates on later data, never the reverse.

            Whatever the scheme, every preprocessing step fitted from data - imputation, scaling, encoding,
            feature selection - must be fitted inside each fold, not once beforehand. Fitting a scaler on the
            full dataset before cross-validating leaks the validation fold into training and inflates every
            score.

            Finally, keep a baseline. Predicting the majority class, or the training mean, sets the bar any
            real model must clear. A sophisticated model that fails to beat the baseline has told you something
            important about the problem.
            """),
        NewExercise(
            "Read a confusion matrix",
            """
            A fraud model produces: 40 true positives, 10 false positives, 20 false negatives, 930 true
            negatives.

            1. Compute accuracy, precision, recall and F1.
            2. State which metric matters most for fraud detection, and why.
            3. The threshold is lowered so more transactions are flagged. Say which metrics rise and which
               fall.
            4. Explain why accuracy here is misleading.
            """,
            """
            Accuracy 0.970, precision 0.800, recall 0.667, F1 0.727. Recall matters most, because an undetected
            fraud costs far more than a reviewed false alarm. Lowering the threshold raises recall and lowers
            precision. Accuracy misleads because 94% of transactions are legitimate, so always predicting
            "legitimate" would already score 0.94 while catching no fraud at all.
            """),
        NewQuiz(
            "Evaluation check",
            Choice(
                "Precision answers which question?",
                ("Of everything flagged positive, what share really was positive?", true),
                ("Of everything that really was positive, what share was caught?", false),
                ("What share of all predictions were correct?", false),
                ("How well does the model rank positives above negatives?", false)),
            Choice(
                "For screening a treatable but dangerous disease, the priority metric is:",
                ("Recall, because a missed case is far costlier than a false alarm", true),
                ("Precision, because false alarms are unacceptable", false),
                ("Accuracy, because it summarises everything", false),
                ("Training loss", false)),
            TrueFalse(
                "A ROC-AUC of 0.5 means the model is no better than random guessing.",
                true),
            Choice(
                "The main advantage of k-fold cross-validation over a single split is:",
                ("A more stable estimate, plus a spread showing how much to trust it", true),
                ("It trains k times faster", false),
                ("It removes the need for a test set entirely", false),
                ("It guarantees the model will not overfit", false)),
            Multiple(
                "Which practices keep a performance estimate honest?",
                ("Fit scalers and imputers inside each fold rather than once beforehand", true),
                ("Choose the evaluation metric before training, not after seeing scores", true),
                ("Compare against a majority-class or mean baseline", true),
                ("Tune hyperparameters directly on the final test set", false))));

    private static Module UnsupervisedLearning() => NewModule(
        "Unsupervised Learning and Next Steps",
        "Finding structure without labels, reducing dimensions, and moving a model from notebook to production.",
        50,
        aiAvatarEnabled: false,
        NewLesson(
            "Clustering and dimensionality reduction",
            """
            Without labels there is no accuracy to compute, so unsupervised methods are judged by whether their
            output is useful.

            k-means partitions data into k groups. It places k centres, assigns each point to the nearest, moves
            each centre to the mean of its members, and repeats until nothing moves. Three properties follow
            directly from that procedure: you must choose k in advance; the result depends on where the centres
            started, so it is run several times; and because it assigns by distance, features must be scaled
            first or the largest-range column dominates.

            Choosing k is a judgement call. The elbow method plots within-cluster variance against k and looks
            for the bend where extra clusters stop paying for themselves. The silhouette score measures how
            much better each point fits its own cluster than the next nearest. Neither gives a definitive
            answer, and a k that makes business sense usually beats a k that is marginally better numerically.

            Principal component analysis reduces many correlated columns to a few uncorrelated ones, ordered by
            how much variance each captures. Keeping enough components to explain, say, 95% of variance often
            cuts dimensionality dramatically at little cost. PCA speeds up training, helps visualisation, and
            reduces overfitting - at the price of interpretability, since a principal component is a blend of
            original features and rarely has a natural meaning.

            Anomaly detection is the third common use: model what normal looks like and flag what does not fit.
            It suits fraud, intrusion detection and equipment monitoring, where examples of the abnormal class
            are too rare to learn from directly.
            """),
        NewLesson(
            "From notebook to production",
            """
            A model that only runs in a notebook has not solved anything. Getting to production raises concerns
            that never appear during training.

            The same preprocessing must run at inference. If the code that prepares training data differs at
            all from the code that prepares live requests, predictions are silently wrong. Package the
            transformations and the model together as a single pipeline artefact, and version them as one unit.

            Models go stale. The world changes, and a model trained on last year's behaviour gradually stops
            describing this year's - a phenomenon called drift. Monitor the distribution of incoming features
            and the distribution of predictions, not merely uptime. Falling accuracy is often visible in the
            inputs before anyone reports a problem.

            Decide how predictions are served. Batch scoring on a schedule is simpler, cheaper and adequate
            whenever a few hours of latency is acceptable. Real-time serving behind an API is necessary when
            the prediction gates a live interaction, and it brings latency budgets, autoscaling and
            availability requirements with it.

            Log inputs and predictions together, so that when ground truth arrives later you can measure what
            actually happened rather than what you hoped. And keep the fallback: what the system does when the
            model is unavailable or returns low confidence is a product decision that must be made explicitly,
            before the outage.
            """),
        NewExercise(
            "Choose k and defend it",
            """
            Running k-means on a customer dataset gives these within-cluster sum-of-squares values:

            k=1: 5200, k=2: 2600, k=3: 1500, k=4: 1180, k=5: 1080, k=6: 1010, k=7: 960

            1. Identify the elbow and state the k you would choose.
            2. Explain why continuing to raise k always lowers the value, and why that does not mean higher k
               is better.
            3. Name one non-numeric consideration that should influence the final choice.
            4. Describe what would go wrong if the features were not scaled first.
            """,
            """
            The elbow is at k=3 or 4, where the improvement flattens sharply. The value always falls because
            more centres necessarily place every point nearer to one, reaching zero when k equals the number of
            points - so it cannot be used as a maximisation target. The final choice should also be actionable:
            a marketing team that can support four campaigns is poorly served by nine segments. Unscaled
            features let the widest-range column dominate the distance calculation, so clusters would form
            along that one axis.
            """),
        NewQuiz(
            "Unsupervised learning check",
            Choice(
                "What must you supply to k-means before it runs?",
                ("The number of clusters, k", true),
                ("A labelled training set", false),
                ("A decision threshold", false),
                ("The names of the clusters", false)),
            Choice(
                "The main purpose of principal component analysis is:",
                ("To reduce many correlated features to fewer uncorrelated ones while keeping most variance",
                    true),
                ("To classify points into known categories", false),
                ("To fill in missing values", false),
                ("To balance an imbalanced dataset", false)),
            TrueFalse(
                "Because k-means assigns points by distance, features should be scaled before running it.",
                true),
            Choice(
                "Model drift means:",
                ("The world has changed, so a model trained on older data no longer describes current data",
                    true),
                ("The model file has become corrupted", false),
                ("Training loss increases between epochs", false),
                ("The learning rate was set too high", false)),
            Multiple(
                "Which are unsupervised techniques?",
                ("k-means clustering", true),
                ("Principal component analysis", true),
                ("Anomaly detection based on a model of normal behaviour", true),
                ("Logistic regression", false))));

    private static Module FinalAssessment() => NewModule(
        "Final Assessment",
        "A timed exam covering the whole course: problem framing, data preparation, modelling and evaluation.",
        45,
        aiAvatarEnabled: false,
        NewExam(
            "Machine Learning Foundations - Final Exam",
            Choice(
                "A dataset of emails labelled spam or not spam is used to train a filter. This is:",
                ("Supervised learning, because every example carries a correct answer", true),
                ("Unsupervised learning, because the text has no structure", false),
                ("Reinforcement learning, because the user reacts to each decision", false),
                ("Not machine learning", false)).Worth(2),
            Choice(
                "Computing a scaler's mean over the full dataset before splitting causes:",
                ("Data leakage, inflating the held-out score", true),
                ("Underfitting, because the scale is wrong", false),
                ("A shape mismatch at inference", false),
                ("Nothing, provided the split is random", false)).Worth(2),
            Choice(
                "Ordinal encoding is appropriate for which column?",
                ("Subscription tier: small, medium, large", true),
                ("Country of residence", false),
                ("Favourite colour", false),
                ("Customer identifier", false)).Worth(2),
            Multiple(
                "A model scores 0.98 on training data and 0.62 on validation data. Which responses are sensible?",
                ("Add regularisation", true),
                ("Gather more training data", true),
                ("Reduce model complexity", true),
                ("Increase model capacity so it fits the training set even better", false)).Worth(3),
            Choice(
                "For a fraud detector where a missed fraud costs far more than a reviewed false alarm, optimise:",
                ("Recall", true),
                ("Precision", false),
                ("Accuracy", false),
                ("Training loss", false)).Worth(2),
            TrueFalse(
                "On a dataset where 99% of rows share one class, high accuracy alone shows the model is useful.",
                false).Worth(2),
            Choice(
                "The purpose of the validation set, separate from the test set, is:",
                ("To choose models and tune hyperparameters, so the test estimate stays honest", true),
                ("To increase the amount of training data", false),
                ("To detect corrupted rows", false),
                ("To measure inference latency", false)).Worth(2),
            Multiple(
                "Which are true of principal component analysis?",
                ("It produces uncorrelated components ordered by variance explained", true),
                ("It can substantially reduce dimensionality at modest cost in information", true),
                ("It reduces interpretability, since components blend original features", true),
                ("It requires labelled data", false)).Worth(3)));
}
