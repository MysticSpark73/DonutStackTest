namespace DonutStack.Data.Score
{
    public class ScoreManager
    {
        private int emptyStackPoints = 10;
        private int completeStackPoints = 50;
        public ScoreManager() { }

        public void AddEmptyStackScore() => Parameters.Parameters.AddScore(emptyStackPoints);

        public void AddCompleteStackScore() => Parameters.Parameters.AddScore(completeStackPoints);

        public void CallReset() => Parameters.Parameters.ResetScore();
    }
}