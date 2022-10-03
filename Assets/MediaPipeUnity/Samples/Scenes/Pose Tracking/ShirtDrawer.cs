using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ShirtDrawer : MonoBehaviour
{
  private enum State { NOT_TRACKING, TRACKING, DRAWING };
  private State state;

  public GameObject prefab;
  public Vector3 offset;
  public float scalar = 1.0f;

  public Mediapipe.Unity.PoseLandmarkListAnnotation poseLandmarks;
  public Mediapipe.Unity.PoseTracking.PoseTrackingSolution solution;

  protected Queue<Vector3> positionDataQueue = new Queue<Vector3>();
  public int positionFilterLength = 3; //you could change it in inspector
  protected Queue<Vector3> upDataQueue = new Queue<Vector3>();
  public int upFilterLength = 3; //you could change it in inspector
  protected Queue<Vector3> forwardDataQueue = new Queue<Vector3>();
  public int forwardFilterLength = 3; //you could change it in inspector

  // public Animator overlayAnimator;

  private Coroutine activeCoroutine;

  private bool isTrackingTimerSuccess = false;

  private GameObject shirt;

  // Start is called before the first frame update
  void Start()
  {
    shirt = Instantiate(prefab);
    shirt.SetActive(false);
    state = State.NOT_TRACKING;

    activeCoroutine = null;
  }

  // Update is called once per frame
  void Update()
  {
    // Enable Tracking if the hand is visible and we are not currently tracking
    if (IsMediaPipeTracking())
    {
      if (state == State.NOT_TRACKING)
      {
        OnTrackingStart();
      }
    }
    // Disable Tracking if the hand is not visilble and we are currently tracking or drawing
    else
    {
      if (state == State.TRACKING)
      {
        OnTrackingStop();
      }
      else if (state == State.DRAWING)
      {
        OnTrackingStop();
      }
    }

    // Draw the shirt
    if (state == State.DRAWING)
    {
      Drawshirt();

    }

  }

  public Vector3 LowPassFilter(Vector3 curFrame, Queue<Vector3> dataQueue, int queueLen)
  {
    if (queueLen <= 0)
      return curFrame;

    if (dataQueue.Count < queueLen)
      dataQueue.Enqueue(curFrame);
    if (dataQueue.Count > queueLen)
      dataQueue.Dequeue();

    dataQueue.Enqueue(curFrame);
    dataQueue.Dequeue();

    Vector3 vFiltered = Vector3.zero;
    foreach (Vector3 v in dataQueue)
    {
      vFiltered += v;
    }

    vFiltered /= queueLen;
    return vFiltered;
  }

  public void OnTrackingStart()
  {
    if (activeCoroutine == null)
    {
      activeCoroutine = StartCoroutine(_OnTrackingStart());
    }
  }
  private IEnumerator _OnTrackingStart()
  {
    //Debug.Log("Tracking Timer Started");

    yield return StartCoroutine(timer(true, 0.5f));

    if (isTrackingTimerSuccess)
    {
      //Debug.Log("Tracking Started");
      state = State.TRACKING;
      // overlayAnimator.SetTrigger("FadeToInformation");
    }

    activeCoroutine = null;
  }
  public void OnTrackingStop()
  {
    if (activeCoroutine == null)
    {
      activeCoroutine = StartCoroutine(_OnTrackingStop());
    }
  }
  private IEnumerator _OnTrackingStop()
  {
    //Debug.Log("Tracking Timer Stopping");

    yield return StartCoroutine(timer(false, 0.1f));

    if (isTrackingTimerSuccess)
    {
      //Debug.Log("Tracking Stopped");
      state = State.NOT_TRACKING;

      OnDrawStop();
      // overlayAnimator.SetTrigger("FadeToError");
    }

    activeCoroutine = null;
  }

  IEnumerator timer(bool tracking, float time)
  {
    DateTime start = DateTime.Now;
    DateTime end = start.AddSeconds(time);

    isTrackingTimerSuccess = false;
    while (DateTime.Now < end)
    {
      if (IsMediaPipeTracking() != tracking) { yield break; }
      yield return null;
    }

    isTrackingTimerSuccess = true;
  }

  public void OnDrawStart()
  {
    //Debug.Log("Drawing Started");
    state = State.DRAWING;
    shirt.SetActive(true);
    Drawshirt();
  }
  public void OnDrawStop()
  {
    //Debug.Log("Drawing Stopped");

    shirt.SetActive(false);
  }

  private bool IsMediaPipeTracking()
  {
    return (poseLandmarks[0]);
  }

  public void Drawshirt()
  {
    // Calculate Transform Vectors and Scale from hand
    //Vector3 up = handLandmarks[0][9].transform.position - handLandmarks[0][0].transform.position;
    //Vector3 left = handLandmarks[0][17].transform.position - handLandmarks[0][5].transform.position;

    Vector3 up = poseLandmarks[15].transform.position - poseLandmarks[13].transform.position;
    Vector3 left = poseLandmarks[0].transform.position - poseLandmarks[5].transform.position;

    Vector3 forward = Vector3.Cross(left, up);
    Vector3 scale = Vector3.one * left.magnitude * scalar;

    up.Normalize();
    left.Normalize();
    forward.Normalize();

    // Apply dampening filter on position and rotation
    //shirt.transform.localPosition = LowPassFilter(handLandmarks[0][0].transform.position, positionDataQueue, positionFilterLength);
    Vector3 pos = poseLandmarks[15].transform.position;
    pos.z = 0;
    shirt.transform.localPosition = LowPassFilter(poseLandmarks[15].transform.position, positionDataQueue, positionFilterLength);
    shirt.transform.up = LowPassFilter(up, upDataQueue, upFilterLength);
    shirt.transform.rotation = Quaternion.LookRotation(LowPassFilter(forward, forwardDataQueue, forwardFilterLength), shirt.transform.up);
    shirt.transform.Translate(new Vector3(offset.x, offset.y, 0));
    shirt.transform.Translate(new Vector3(0, 0, offset.z), Space.World);

    // Apply Scale
    shirt.transform.localScale = scale;

  }
}
