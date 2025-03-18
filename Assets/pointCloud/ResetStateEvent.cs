using System;

public class ResetStateEvent
{
    // 리셋 상태가 변경되었을 때 호출되는 이벤트
    public static event Action<bool> OnResetStateChanged;

    // 리셋 상태를 방송하는 메서드
    public static void BroadcastResetState(bool isActive)
    {
        OnResetStateChanged?.Invoke(isActive); // 이벤트를 호출하여 모든 구독자에게 알림
    }
}
