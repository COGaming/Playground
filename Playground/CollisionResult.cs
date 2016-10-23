namespace Playground
{
    public struct CollisionResult
    {
        public bool IsPushingTop { get; private set; }

        public bool IsPushingBottom { get; private set; }

        public bool IsPushingLeft { get; private set; }

        public bool IsPushingRight { get; private set; }

        public CollisionResult(bool isPushingTop, bool isPushingBottom, bool isPushingLeft, bool isPushingRight)
        {
            IsPushingTop = isPushingTop;
            IsPushingBottom = isPushingBottom;
            IsPushingLeft = isPushingLeft;
            IsPushingRight = isPushingRight;
        }
    }
}