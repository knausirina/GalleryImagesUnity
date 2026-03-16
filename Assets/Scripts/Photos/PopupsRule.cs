using System;
using Zenject;

public class PopupsRule : IInitializable, IDisposable
{
    private readonly SignalBus _signalBus;
    private readonly PopupsStorage _popupsStorage;

    public PopupsRule(SignalBus signalBus, PopupsStorage popupsStorage)
    {
        _signalBus = signalBus;
        _popupsStorage = popupsStorage;
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<PhotoClickedSignal>(OnPhotoClickedSignal);
    }

    public void Initialize()
    {
        _signalBus.Subscribe<PhotoClickedSignal>(OnPhotoClickedSignal);
    }

    private void OnPhotoClickedSignal(PhotoClickedSignal signal)
    {
        if (signal.IsPremium)
            _popupsStorage.GetView<PremiumPhotoPopup>().Show();
        else
        {
            var popup = _popupsStorage.GetView<SimplePhotoPopup>();
            popup.SetData(signal.Index);
            popup.Show();
        }
    }
}