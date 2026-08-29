namespace The_Timestopper.Internal
{
    public interface IFixedUpdateReceiver
    {
        bool isRegistered { get; set; }
        void FakeFixedUpdate();
    }
}