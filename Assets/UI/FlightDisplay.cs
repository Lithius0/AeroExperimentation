using TMPro;
using UnityEngine;

public class FlightDisplay : MonoBehaviour
{
    [SerializeField]
    private FlightModel flightModel;
    [SerializeField]
    private TMP_Text speedLabel;
    [SerializeField]
    private TMP_Text altitudeLabel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        speedLabel.text = $"SPD: {flightModel.Velocity.magnitude:0.0}";
        altitudeLabel.text = $"ALT: {flightModel.transform.position.y:0.0}";
    }
}
