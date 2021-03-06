using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

public enum Direction
{ 
    kLeft,
    kRight,
    kUp,
    kDown,

    kMax
}

public class GridMovementBase : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] Animator m_animator;
    [SerializeField] SpriteRenderer m_spriteRenderer;


    [Header("Movement")]
    [SerializeField] GridMovementBase m_partner;
    [SerializeField] Tilemap m_walkableTilemap;
    [SerializeField] protected float m_moveSpeed = 1.0f;

    [Header("Audio")]
    [SerializeField] AudioClip m_stepSound;

    [Header("UI")]
    [SerializeField] StepCounterBehavior m_stepsText;

    private AudioSource m_audioSource;

    private bool m_isAtGoal = false;

    //Location Data
    protected Direction m_direction;
    protected Vector3Int m_position;

    protected bool m_atDestination = false;
    protected bool m_canMove = true;
    protected bool m_isSpinning = false;
    [SerializeField] protected bool m_controlsInverted = false;

    protected Vector3 m_targetPosition;
    protected Vector3 m_intendedTargetPosition;

    protected int m_steps = 0;

    public void SetAtGoal(bool newAtGoal)
    {
        m_isAtGoal = newAtGoal;
    }

    public bool GetAtGoal() { return m_isAtGoal; }

    public void SetCanMove(bool newCanMove) { m_canMove = newCanMove; }
    public bool GetCanMove() { return m_canMove; }

    public void SetTargetPosition(Vector3 newPos) { m_targetPosition = newPos; }
    public Vector3 GetTargetPosition() { return m_targetPosition; }

    public void Spin() { m_animator.SetBool("isSpinning", true); m_isSpinning = true; }

    void Start()
    {
        m_position = m_walkableTilemap.WorldToCell(transform.position);

        m_audioSource = GetComponent<AudioSource>();   
    }

    void Update()
    {
        if (!m_canMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_targetPosition, m_moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, m_targetPosition) < .001f)
            {
                m_canMove = true;
                UpdatePosition();

                m_animator.SetBool("isWalking", false);

                if (m_isSpinning)
                {
                    m_isSpinning = false;
                    m_animator.SetBool("isSpinning", false);
                }
            }

        }

        HandleInput();
    }

    protected void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move(Direction.kUp);
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(Direction.kDown);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(Direction.kLeft);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Direction.kRight);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    protected void Move(Direction dir)
    {
        if (!m_canMove) return;

        if(m_stepsText != null)
            m_stepsText.ChangeSore(++m_steps);

        switch (dir)
        {
            case Direction.kLeft:
                m_animator.SetBool("isFacingUp", false);
                m_animator.SetBool("isFacingDown", false);
                m_spriteRenderer.flipX = true;
                if (m_controlsInverted)
                {
                    TryMove(new Vector3Int(m_position.x + 1, m_position.y, (int)transform.position.z));
                    m_spriteRenderer.flipX = false;
                }
                else
                {
                    TryMove(new Vector3Int(m_position.x - 1, m_position.y, (int)transform.position.z));
                    m_spriteRenderer.flipX = true;
                }
                break;
            case Direction.kRight:
                m_animator.SetBool("isFacingUp", false);
                m_animator.SetBool("isFacingDown", false);
                if (m_controlsInverted)
                {
                    TryMove(new Vector3Int(m_position.x - 1, m_position.y, (int)transform.position.z));
                    m_spriteRenderer.flipX = true;
                }
                else
                {
                    TryMove(new Vector3Int(m_position.x + 1, m_position.y, (int)transform.position.z));
                    m_spriteRenderer.flipX = false;
                }
                break;
            case Direction.kUp:
                m_animator.SetBool("isFacingUp", true);
                m_animator.SetBool("isFacingDown", false);
                TryMove(new Vector3Int(m_position.x, m_position.y + 1, (int)transform.position.z));
                break;
            case Direction.kDown:
                m_animator.SetBool("isFacingUp", false);
                m_animator.SetBool("isFacingDown", true);
                TryMove(new Vector3Int(m_position.x, m_position.y - 1, (int)transform.position.z));
                break;
            case Direction.kMax:
                break;
        }
    }

    protected bool TryMove(Vector3Int target)
    {
        m_intendedTargetPosition = m_walkableTilemap.GetCellCenterWorld(target);
        var targetTile = m_walkableTilemap.GetTile(m_walkableTilemap.WorldToCell(m_intendedTargetPosition));
        if (targetTile != null)
        {
            m_canMove = false;
            m_targetPosition = m_intendedTargetPosition;

            m_animator.SetBool("isWalking", true);

            m_audioSource.Play();
            return true;
        }
        else
        {
            m_intendedTargetPosition = Vector3Int.zero;
            return false;
        }
    }

    void UpdatePosition()
    {
        m_position = m_walkableTilemap.WorldToCell(transform.position);
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
