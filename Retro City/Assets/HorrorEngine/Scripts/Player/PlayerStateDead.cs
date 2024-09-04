
namespace HorrorEngine
{
    public class PlayerStateDead : ActorState
    {
        public override void StateEnter(IActorState fromState)
        {
            Actor.Disable(this);

            UIManager.Get<UIInputListener>().AddBlockingContext(this);

            base.StateEnter(fromState);
        }


        public override void StateExit(IActorState intoState)
        {
            Actor.Enable(this);

            UIManager.Get<UIInputListener>().RemoveBlockingContext(this);

            base.StateExit(intoState);
        }
    }
}