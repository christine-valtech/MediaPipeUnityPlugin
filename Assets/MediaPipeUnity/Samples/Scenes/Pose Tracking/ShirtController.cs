using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShirtController : MonoBehaviour
{
  public Mediapipe.Unity.PoseTracking.PoseTrackingSolution solution;
  private DeviceOrientation curOrientation;

  // Start is called before the first frame update
  void Start()
  {
    curOrientation = Input.deviceOrientation;
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.deviceOrientation != curOrientation)
    {
      switch (Input.deviceOrientation)
      {
        case DeviceOrientation.Portrait:
        case DeviceOrientation.PortraitUpsideDown:
          //Debug.Log("Orientation Changed to Portrait");
          solution.Play();
          break;
        case DeviceOrientation.LandscapeLeft:
        case DeviceOrientation.LandscapeRight:
          //Debug.Log("Orientation Changed to Landscape");
          solution.Play();
          break;
        default:
          //Debug.Log("Orientation Changed to Untracked");
          break;

      }
      curOrientation = Input.deviceOrientation;
    }
  }

  void FlipCamera()
  {

  }
}
