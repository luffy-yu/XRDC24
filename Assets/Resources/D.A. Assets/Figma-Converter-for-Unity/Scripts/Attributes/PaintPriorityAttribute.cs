using System;

namespace DA_Assets.FCU.Attributes
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class PaintPriorityAttribute : Attribute
    {
        public int Priority { get; }

        public PaintPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}