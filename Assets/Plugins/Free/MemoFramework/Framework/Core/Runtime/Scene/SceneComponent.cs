namespace MemoFramework
{
    public partial class SceneComponent : MemoFrameworkComponent
    {
        public bool IsLoadingScene { get; private set; }
        public BuiltInSceneLoader BuiltIn { get; private set; }
        public AddressableSceneLoader Addressable { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            BuiltIn = new BuiltInSceneLoader(this);
            Addressable = new AddressableSceneLoader(this);
        }
    }
}