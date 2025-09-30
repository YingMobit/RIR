using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ECSStatsPanel : MonoBehaviour {
    [SerializeField] private Runner runner; // 场景中的 Runner 实例（若为空会在 Awake 尝试查找）
    [SerializeField] private Text entityCountText;
    [SerializeField] private Text activeEntityCountText;
    [SerializeField] private Text componentCountText;
    [SerializeField] private Text activeComponentCountText;

    [Tooltip("UI 刷新间隔，秒（减少频繁更新带来的开销）")]
    [SerializeField] private float updateInterval = 0.2f;

    private float _timer;
    private StringBuilder _sb = new StringBuilder(128);

    private void Awake() {
        if(runner == null) {
            runner = GameObject.FindAnyObjectByType<Runner>();
        }
    }

    private void Update() {
        if(runner == null)
            return;
        _timer += Time.deltaTime;
        if(_timer < updateInterval)
            return;
        _timer = 0f;
        Refresh();
    }

    private void Refresh() {
        // 避免多次分配，使用 StringBuilder
        _sb.Clear();
        _sb.Append("实体总数: ").Append(runner.CurrentEntityCount);
        if(entityCountText != null)
            entityCountText.text = _sb.ToString();

        _sb.Clear();
        _sb.Append("活跃实体: ").Append(runner.CurrentActiveEntityCount);
        if(activeEntityCountText != null)
            activeEntityCountText.text = _sb.ToString();

        _sb.Clear();
        _sb.Append("组件总数: ").Append(runner.CurrentComponentCount);
        if(componentCountText != null)
            componentCountText.text = _sb.ToString();

        _sb.Clear();
        _sb.Append("活跃组件: ").Append(runner.CurrentActiveComponentCount);
        if(activeComponentCountText != null)
            activeComponentCountText.text = _sb.ToString();
    }
}