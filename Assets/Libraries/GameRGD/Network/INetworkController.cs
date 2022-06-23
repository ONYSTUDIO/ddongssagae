namespace DoubleuGames.GameRGD
{
    public interface INetworkController
    {
        bool Login(CGameNetwork network);
        void OnLoginReceive(CGameNetwork network);
    }
}