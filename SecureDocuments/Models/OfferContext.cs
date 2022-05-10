namespace SecureDocuments.Models
{
    public class OfferContext
    {
        public OfferState State { get; set; }

        public OfferContext(OfferState state)
        {
            TransitionTo(state);
        }

        public OfferState TransitionTo(OfferState state)
        {
            Console.WriteLine($"Context: Transition to {state.GetType().Name}.");
            state.SetContext(this);
            return state;
        }

        internal OfferState ChangedTo(Status status)
        {
            var state = status switch
            {
                Status.All => TransitionTo(new DefaultState()),
                Status.NEW => TransitionTo(new NewState()),
                Status.REJECTED => TransitionTo(new RejectedState()),
                Status.ACCEPTED => TransitionTo(new AcceptedState()),
                Status.FINISHED => TransitionTo(new FinishedState()),
                Status.ARCHIVED => TransitionTo(new ArchivedState()),
                _ => throw new NotImplementedException(),
            };
            State = state;
            return state;
        }
    }

    public abstract class OfferState
    {
        public Status Status { get; init; }
        public bool CanAccept { get; init; }
        public bool CanReject { get; init; }
        public bool CanFinish { get; init; }
        public bool CanArchive { get; init; }
        public bool CanManagedFiles { get; init; }

        protected WeakReference<OfferContext>? _context;

        public void SetContext(OfferContext context)
        {
            _context = new WeakReference<OfferContext>(context);
        }

        public abstract void NextState();

        public abstract void PrevState();
    }

    public class DefaultState : OfferState
    {
        public DefaultState()
        {
            Status = Status.All;
            CanAccept = false;
            CanReject = false;
            CanFinish = false;
            CanArchive = false;
            CanManagedFiles = false;
        }
        public override void NextState()
        {
            Console.WriteLine("DefaultState handles NextState.");
            Console.WriteLine("DefaultState wants to change the state of the context to NewState.");
            if (_context != null && _context.TryGetTarget(out OfferContext? offerContext) && offerContext != null)
            {
                offerContext.TransitionTo(new NewState());
            }
        }

        public override void PrevState()
        {
            Console.WriteLine("DefaultState handles PrevState.");
        }
    }

    public class NewState : OfferState
    {
        public NewState()
        {
            Status = Status.NEW;
            CanAccept = true;
            CanReject = true;
            CanFinish = false;
            CanArchive = false;
            CanManagedFiles = true;
        }
        public override void NextState()
        {
            Console.WriteLine("NewState handles NextState.");
            //_context?.TransitionTo(new NewState());
        }

        public override void PrevState()
        {
            Console.WriteLine("NewState handles PrevState.");
        }
    }

    public class RejectedState : OfferState
    {
        public RejectedState()
        {
            Status = Status.REJECTED;
            CanAccept = true;
            CanReject = false;
            CanFinish = false;
            CanArchive = false;
            CanManagedFiles = true;
        }
        public override void NextState()
        {
            Console.WriteLine("RejectedState handles NextState.");
            //_context?.TransitionTo(new NewState());
        }

        public override void PrevState()
        {
            Console.WriteLine("RejectedState handles PrevState.");
        }
    }

    public class AcceptedState : OfferState
    {
        public AcceptedState()
        {
            Status = Status.ACCEPTED;
            CanAccept = false;
            CanReject = false;
            CanFinish = true;
            CanArchive = false;
            CanManagedFiles = true;
        }
        public override void NextState()
        {
            Console.WriteLine("AcceptedState handles NextState.");
            //_context?.TransitionTo(new NewState());
        }

        public override void PrevState()
        {
            Console.WriteLine("AcceptedState handles PrevState.");
        }
    }

    public class FinishedState : OfferState
    {
        public FinishedState()
        {
            Status = Status.FINISHED;
            CanAccept = false;
            CanReject = false;
            CanFinish = false;
            CanArchive = true;
            CanManagedFiles = true;
        }
        public override void NextState()
        {
            Console.WriteLine("FinishedState handles NextState.");
            //_context?.TransitionTo(new NewState());
        }

        public override void PrevState()
        {
            Console.WriteLine("FinishedState handles PrevState.");
        }
    }

    public class ArchivedState : OfferState
    {
        public ArchivedState()
        {
            Status = Status.ARCHIVED;
            CanAccept = false;
            CanReject = false;
            CanFinish = true;
            CanArchive = false;
            CanManagedFiles = false;
        }
        public override void NextState()
        {
            Console.WriteLine("ArchivedState handles NextState.");
            //_context?.TransitionTo(new NewState());
        }

        public override void PrevState()
        {
            Console.WriteLine("ArchivedState handles PrevState.");
        }
    }

}
