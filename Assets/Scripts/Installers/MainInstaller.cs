using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
    [SerializeField] private GalleryViewController galleryView;
    [SerializeField] private Config _config;
    [SerializeField] private GameObject _photoPrefab;

    [SerializeField] private PopupsConfig _popupsConfig;
    [SerializeField] private GameObject _popupsRoot;

    public override void InstallBindings()
    {
        RegisterSignals();

        Container.BindInterfacesAndSelfTo<GalleryViewController>().FromInstance(galleryView).AsSingle();

        Container.BindInstance(_config).AsSingle();
        Container.BindInterfacesAndSelfTo<GalleryController>().AsSingle();
        Container.BindInterfacesAndSelfTo<ImageProvider>().AsSingle();


        Container.Bind<PopupsStorage>().AsSingle().WithArguments(_popupsConfig, _popupsRoot);
        Container.BindInterfacesAndSelfTo<PhotoPopupsController>().AsSingle();

        Container.BindInterfacesAndSelfTo<PhotoPool>().AsSingle().WithArguments(Container, _photoPrefab, _config.InitialPhotoPoolCapacity);
    }

    private void RegisterSignals()
    {
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<FilterChangedSignal>();
        Container.DeclareSignal<PhotoClickedSignal>();
    }
}