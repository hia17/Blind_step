/// <summary>
/// 같은 프레임 안에서 입력이 이미 소비됐는지를 공유하는 정적 헬퍼.
/// ItemPickupDetector가 먼저 입력을 소비하면 ObjectTrigger가 이를 감지해 동작을 건너뜁니다.
/// </summary>
public static class InputConsumer
{
    private static int _consumedFrame = -1;

    /// <summary>이번 프레임에 입력을 소비(선점)합니다.</summary>
    public static void Consume()
    {
        _consumedFrame = UnityEngine.Time.frameCount;
    }

    /// <summary>이번 프레임에 이미 입력이 소비됐으면 true.</summary>
    public static bool IsConsumed => UnityEngine.Time.frameCount == _consumedFrame;
}