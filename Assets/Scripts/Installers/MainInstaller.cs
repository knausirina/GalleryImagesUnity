using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
    [SerializeField] private GalleryViewController _galleryView;
    [SerializeField] private Config _config;
    [SerializeField] private GameObject _photoPrefab;
    [SerializeField] private GameObject _placePhotoPrefab;

    [SerializeField] private PopupsConfig _popupsConfig;
    [SerializeField] private GameObject _popupsRoot;

    public override void InstallBindings()
    {
        RegisterSignals();

        Container.BindInterfacesAndSelfTo<GalleryViewController>().FromInstance(_galleryView).AsSingle();

        Container.BindInstance(_config).AsSingle();
        Container.BindInterfacesAndSelfTo<GalleryController>().AsSingle().NonLazy();
        Container.Bind<GalleryFilterManager>().AsSingle().NonLazy();
        Container.Bind<PhotoLoader>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ImageProvider>().AsSingle().NonLazy();
        Container.Bind<PhotoService>().AsSingle().NonLazy();


        Container.Bind<PopupsStorage>().AsSingle().WithArguments(_popupsConfig, _popupsRoot);
        Container.BindInterfacesAndSelfTo<PopupsRule>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<PhotoPool>().AsSingle().WithArguments(_placePhotoPrefab, _photoPrefab, _config.InitialPhotoPoolCapacity);
    }

    private void RegisterSignals()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<FilterChangedSignal>();
        Container.DeclareSignal<PhotoClickedSignal>();
    }
}