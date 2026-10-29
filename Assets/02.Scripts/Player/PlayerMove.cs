using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMove : MonoBehaviour
{
    private float DodgeTime = 0.5f;

    [Header("Stat")]
    [SerializeField] private float _moveSpeed = 5f;

    [SerializeField] private float _dodgeSpeed = 30f;

    [SerializeField] private float _jumpSpeed = 10f; // 음 이거 왜있음? 

    private float _currentSpeed = 0f;
    private bool _isDodge = false;
    private float _moveSpeedBonusRate = 0f; // 증강으로 늘어난 이동 속도 비율

    public float MoveSpeed => _moveSpeed;


    [Header("Components")]
    private CharacterController _controller;

    [SerializeField] private Transform _cameraTransform;

    private Player _player;
    private float _baseMoveSpeed;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _player = GetComponent<Player>();

        _baseMoveSpeed = _moveSpeed;
        _currentSpeed = _moveSpeed;
    }

    private void Update()
    {
        // 기절 중에는 이동/회피 불가
        if (_player != null && _player.IsStunned)
        {
            return;
        }

        // 1. 입력 받는다.
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // 2. 방향을 구한다.
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // 카메라좌우 기준만 이동 방향으로 
        direction = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f) * direction;

        if (Input.GetButtonDown("Dodge"))
        {
            Debug.Log("Dodge Start");
            StartCoroutine(Dodge());
        }

        float slowRate = _player != null && _player.IsMoveSlowed ? _player.MoveSlowRate : 1f;
        Vector3 velocity = direction * _currentSpeed * slowRate * Time.deltaTime;
        _controller.Move(velocity);
    }

    public Transform GetCameraTransform()
    {
        if (_cameraTransform != null)
        {
            return _cameraTransform;
        }
        else
        {
            Debug.Log("CameraTrasform is null");
            return null;
        }
    }

    // 증강: 이동 속도 +rate (기본값 대비 누적)
    public void UpgradeMoveSpeed(float rate)
    {
        _moveSpeedBonusRate += rate;
        _moveSpeed = _baseMoveSpeed * (1f + _moveSpeedBonusRate);
        if (!_isDodge)
        {
            _currentSpeed = _moveSpeed;
        }
    }

    public void ResetStats()
    {
        StopAllCoroutines();
        _isDodge = false;
        _moveSpeedBonusRate = 0f;
        _moveSpeed = _baseMoveSpeed;
        _currentSpeed = _moveSpeed;
    }

    private IEnumerator Dodge()
    {
        _isDodge = true;
        _currentSpeed = _dodgeSpeed;

        yield return new WaitForSeconds(DodgeTime);

        _isDodge = false;
        _currentSpeed = _moveSpeed;
    }
}