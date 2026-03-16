using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = -360f;

    private void OnEnable()
    {
        RotateAsync(destroyCancellationToken).Forget();
    }

    private async UniTaskVoid RotateAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                transform.Rotate(0, 0, _rotationSpeed * Time.deltaTime);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException) {
        }
    }
}