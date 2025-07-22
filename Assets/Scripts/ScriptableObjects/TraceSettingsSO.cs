using UnityEngine;

[CreateAssetMenu(fileName = "TraceSettingsSO", menuName = "SO/TraceSettings")]
public class TraceSettingsSO : ScriptableObject
{
    [SerializeField] private float _traceUnitLifetime;
    [SerializeField] private float _traceUnitSpawnRate;
    [SerializeField] private float _startingAlpha;
    [SerializeField] private Color _traceColor;
    [SerializeField] private bool _drawTrace = true;

    public float TraceUnitLifetime => _traceUnitLifetime;
    public float TraceUnitSpawnRate => _traceUnitSpawnRate;
    public float StartingAlpha => _startingAlpha;
    public Color TraceColor => _traceColor;
    public bool DrawTrace => _drawTrace;
}
