using System.Diagnostics;

namespace Ascension.Networking
{
    public struct SceneLoadState
    {
        public const int STATE_LOADING = 1;
        public const int STATE_LOADING_DONE = 2;
        public const int STATE_CALLBACK_INVOKED = 3;

        public int State;
        public Scene Scene;
        public IMessageRider Token;

        public SceneLoadState BeginLoad(int index, IMessageRider token)
        {
            return new SceneLoadState { Scene = new Scene(index, (this.Scene.Sequence + 1) & 255), State = SceneLoadState.STATE_LOADING, Token = token };
        }

        public static SceneLoadState DefaultRemote()
        {
            return new SceneLoadState() { Scene = new Scene(255, 255), State = SceneLoadState.STATE_CALLBACK_INVOKED, Token = null };
        }

        public static SceneLoadState DefaultLocal()
        {
            return new SceneLoadState() { Scene = new Scene(255, 255), State = SceneLoadState.STATE_LOADING_DONE, Token = null };
        }

        public void Set(SceneLoadState state)
        {
            State = state.State;
            Scene = new Scene(state.Scene.Index, state.Scene.Sequence);
            Token = state.Token;
        }
    }

    public struct Scene
    {
        public readonly int Index;
        public readonly int Sequence;

        public Scene(int index, int sequence)
        {
            NetAssert.True(index == (index & 255));
            NetAssert.True(sequence == (sequence & 255));

            this.Index = index & 255;
            this.Sequence = sequence & 255;
        }

        public override int GetHashCode()
        {
            return Sequence ^ Index;
        }

        public override bool Equals(object obj)
        {
            return ((Scene)obj) == this;
        }

        public override string ToString()
        {
            return string.Format("[Scene Index={0} Sequence={1}]", Index, Sequence);
        }

        public static bool operator ==(Scene a, Scene b)
        {
            return (a.Index == b.Index) && (a.Sequence == b.Sequence);
        }

        public static bool operator !=(Scene a, Scene b)
        {
            return (a.Index != b.Index) || (a.Sequence != b.Sequence);
        }
    }
}