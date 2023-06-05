namespace Dip.Lens.Server.Hubs
{
    public interface ILogisticClient
    {
        public Task SendMessage(string user, string message);
    }
}
