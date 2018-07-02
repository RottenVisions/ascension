
namespace Ascension.Networking
{
    public class SceneLoadChannel : Channel
    {
        public override void Pack(Packet packet)
        {
            var s = packet;
            s.WriteBool(Core.CanReceiveEntities);

            var local = Core.LocalSceneLoading;
            var remote = connection.remoteSceneLoading;

            s.WriteInt(local.State, 2);
            s.WriteInt(local.Scene.Index, 8);
            s.WriteInt(local.Scene.Sequence, 8);

            if (AscensionNetwork.IsServer)
            {
                if (s.WriteBool(local.Scene != remote.Scene))
                {
                    s.WriteToken(local.Token);
                }
            }
        }

        public override void Read(Packet packet)
        {
            var s = packet;

            connection.canReceiveEntities = s.ReadBool();

            SceneLoadState local = Core.LocalSceneLoading;
            SceneLoadState remote = new SceneLoadState();

            remote.State = s.ReadInt(2);
            remote.Scene = new Scene(s.ReadInt(8), s.ReadInt(8));

            if (AscensionNetwork.IsClient)
            {
                if (s.ReadBool())
                {
                    remote.Token = s.ReadToken();
                }
            }

            if (connection.remoteSceneLoading.Scene == remote.Scene)
            {
                remote.State = System.Math.Max(connection.remoteSceneLoading.State, remote.State);
            }

            connection.remoteSceneLoading = remote;

            if (Core.IsClient)
            {
                // if the scene the remote is loading is not the same as ours... we should switch
                if (remote.Scene != Core.LocalSceneLoading.Scene)
                {
                    // set the loading state
                    remote.State = SceneLoadState.STATE_LOADING;

                    // and begin loading
                    Core.LoadSceneInternal(remote);
                }
            }
        }
    }
}

