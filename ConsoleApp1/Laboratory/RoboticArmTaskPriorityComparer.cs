namespace QStandaedPlatform.Engine.Laboratory
{
    public class RoboticArmTaskPriorityComparer : IComparer<RoboticArmTaskPriority>
    {
        public int Compare(RoboticArmTaskPriority x, RoboticArmTaskPriority y)
        {
            return x.CompareTo(y);
        }
    }
}
