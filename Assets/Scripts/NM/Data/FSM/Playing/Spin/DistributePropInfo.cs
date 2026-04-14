using NM.Config;

namespace NM.Data;

public class DistributePropInfo
{
    public required EPropType PropType;
    public required long Value;
    // 若置空则代表直接收获到玩家属性.
    public GamePlaying.MyItem? ToItem;
}