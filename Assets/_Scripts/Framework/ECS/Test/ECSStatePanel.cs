using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ECSStatsPanel : MonoBehaviour {
    [SerializeField] private Runner runner; // �����е� Runner ʵ������Ϊ�ջ��� Awake ���Բ��ң�
    [SerializeField] private Text entityCountText;
    [SerializeField] private Text activeEntityCountText;
    [SerializeField] private Text componentCountText;
    [SerializeField] private Text activeComponentCountText;

    [Tooltip("UI ˢ�¼�����루����Ƶ�����´����Ŀ�����")]
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
        // �����η��䣬ʹ�� StringBuilder
        _sb.Clear();
        _sb.Append("ʵ������: ").Append(runner.CurrentEntityCount);
        if(entityCountText != null)
            entityCountText.text = _sb.ToString();

        _sb.Clear();
        _sb.Append("��Ծʵ��: ").Append(runner.CurrentActiveEntityCount);
        if(activeEntityCountText != null)
            activeEntityCountText.text = _sb.ToString();

        _sb.Clear();
        _sb.Append("�������: ").Append(runner.CurrentComponentCount);
        if(componentCountText != null)
            componentCountText.text = _sb.ToString();

        _sb.Clear();
        _sb.Append("��Ծ���: ").Append(runner.CurrentActiveComponentCount);
        if(activeComponentCountText != null)
            activeComponentCountText.text = _sb.ToString();
    }
}