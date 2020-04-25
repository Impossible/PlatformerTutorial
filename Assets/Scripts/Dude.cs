#define DECCELERATION
#define TURNING
#define COLLISIONX
#define GRAVITY
#define COLLISIONY
#define JUMPING
#define VARIABLEJUMP
#define ANIMATION
#define GRACETIMER
#define JUMPBUFFER
#define PARTICLES
#define SOUND
#define EVENTS

using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class Dude : MonoBehaviour
{
    #region PHYSICS
    [Header("Movement")]
    [Tooltip("Run acceleration in units/second")]
    public float moveAcceleration;
    [Tooltip("Run decceleration in units/second")]
    public float moveDecceleration;
    [Tooltip("Air acceleration in units/second")]
    public float airAcceleration;
    [Tooltip("Maximum velocity")]
    public float maxSpeed;
    [Tooltip("Acceleration while changing direction")]
    public float turnAcceleration;

    [Header("Gravity")]
    [Tooltip("Gravity when not holding jump button")]
    public float gravity;
    [Tooltip("Gravity when holding jump button")]
    public float highJumpGravity;

    [Header("Jumping")]
    [Header("Jump velocity in units/second")]
    public float jumpVelocity;
    [Tooltip("Jump grace time in seconds")]
    public float graceTime;
    [Tooltip("jump buffering time in seconds")]
    public float jumpBufferTime;
    #endregion

    #region CHARACTERSTATE
    bool inAir = false;
    float graceTimer;
    float jumpBufferTimer;
    private Vector3 velocity;
    private Vector3 acceleration;
    private BoxCollider2D hitBox;
    private RaycastHit2D[] hitResult = new RaycastHit2D[1];
    private Vector3 spawnPoint;
    #endregion

    #region PARTICLEEFFECT
    [Header("Particles")]
    public ParticleSystem resetParticle;
    #endregion

    #region DRAWING
    SpriteRenderer spriteRenderer;
    [Header("Animation")]
    public Sprite[] runFrames;
    public Sprite[] jumpFrames;
    public Sprite[] idleFrames;

    Sprite[] currentAnimation;
    int currentFrame;
    float frameTimer;
    bool animationLooping = false;
    public float animationSpeed = 1.0f / 24.0f;
    #endregion

    #region EVENTS
    [System.Serializable]
    public class JumpEvent : UnityEvent<Vector3, float>
    {
    }

    [Header("Jump event")]
    public JumpEvent jumpEvent;
    public Vector3 jumpShakeAmount = new Vector3(0.3f, 0.3f, 0);
    public float jumpShakeTime = 0.2f;
    #endregion

    #region SOUND
    [Header("Sounds")]
    [Tooltip("Sound that plays when jumping")]
    public AudioSource jumpSound;
    [Tooltip("Sound that plays after reset")]
    public AudioSource resetSound;
    #endregion


    private void Awake()
    {
        currentAnimation = idleFrames;
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitBox = GetComponent<BoxCollider2D>();
        inAir = false;
        resetParticle.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnPoint = transform.position;
#if GRAVITY
        acceleration.y = gravity;
#endif
    }

    void SetAnimation(Sprite[] anim, bool looping)
    {
#if ANIMATION
        animationLooping = looping;
        currentFrame = 0;
        currentAnimation = anim;
        frameTimer = 0;
        spriteRenderer.sprite = currentAnimation[currentFrame];
#endif
    }

    void Jump()
    {
#if JUMPING
        velocity.y = jumpVelocity;
        inAir = true;
#if SOUND
        jumpSound.Play();
#endif
        SetAnimation(jumpFrames, false);
#endif
    }

    private void Update()
    {
        #region INPUT
        #region RUNNING
        if (Input.GetKey(KeyCode.RightArrow)) // move right
        {
            acceleration.x = inAir ? airAcceleration : moveAcceleration;
            spriteRenderer.flipX = false;

            if (!inAir)
            {
#if TURNING
                if (Mathf.Sign(velocity.x) != Mathf.Sign(acceleration.x))
                {
                    acceleration.x = turnAcceleration;
                }
#endif
                if (currentAnimation != runFrames)
                {
                    SetAnimation(runFrames, true);
                }
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) // move left
        {
            acceleration.x = inAir ? -airAcceleration : -moveAcceleration;
            spriteRenderer.flipX = true;

            if (!inAir)
            {
#if TURNING
                if (Mathf.Sign(velocity.x) != Mathf.Sign(acceleration.x))
                {
                    acceleration.x = -turnAcceleration;
                }
#endif
                if (currentAnimation != runFrames)
                {
                    SetAnimation(runFrames, true);
                }
            }
        }
        else if (!inAir) // apply friction on the ground
        {
            acceleration.x = 0;
#if DECCELERATION
            velocity.x = Mathf.MoveTowards(velocity.x, 0, Time.fixedDeltaTime * moveDecceleration); // move toward zero at acceleration speed with no input
#else
            velocity.x = 0;
#endif
            if (Mathf.Abs(velocity.x) < Mathf.Epsilon)
            {
                SetAnimation(idleFrames, true);
            }
        }
        #endregion
        #region JUMPING
#if JUMPING
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //jump if on the ground or within the jump grace timer period
            if (!inAir || graceTimer > 0)
            {
                Jump();
                graceTimer = 0;
            }
            else
            {
                // if in air activate jump buffering so the character jumps when they land
                jumpBufferTimer = jumpBufferTime;
            }

        }
#endif
                #region VARIABLEJUMP
#if VARIABLEJUMP
        if (Input.GetKey(KeyCode.Space))
        {
            acceleration.y = highJumpGravity;
        }
        else
        {
            acceleration.y = gravity;
        }
#endif
        #endregion

        #endregion
        #endregion

        #region ANIMATIONUPDATE
        frameTimer += Time.deltaTime;
        if (frameTimer > animationSpeed)
        {
            currentFrame += 1;
            frameTimer = 0;
            if (animationLooping)
            {
                currentFrame = currentFrame % currentAnimation.Length;
            }
            else
            {
                currentFrame = Mathf.Min(currentFrame, currentAnimation.Length - 1);
            }
            spriteRenderer.sprite = currentAnimation[currentFrame];
        }
        #endregion
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        #region PHYSICSUPDATE
        velocity += acceleration * Time.fixedDeltaTime;
        velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed); // limit the character's speed
        #endregion

        Vector3 moveTo = transform.position;

        #region COLLISION
#if COLLISIONY
        if (hitBox.Cast(new Vector2(0, Mathf.Sign(velocity.y)), hitResult, Mathf.Abs(velocity.y) * Time.fixedDeltaTime) > 0)
        {
#if EVENTS
            if (inAir)
            {
                jumpEvent.Invoke(jumpShakeAmount, jumpShakeTime);
            }
#endif

            moveTo.y = hitResult[0].point.y - (hitBox.bounds.extents.y + 0.01f) * Mathf.Sign(velocity.y) - hitBox.offset.y;
            inAir = false;
            velocity.y = 0;
#if JUMPBUFFER
            //if there is a buffered jump make the character jump
            if (jumpBufferTimer > 0)
            {
                Jump();
                jumpBufferTimer = 0;
            }
#endif
        }
        else
#endif
        {
            moveTo.y += velocity.y * Time.fixedDeltaTime;

#if JUMPING
            if (!inAir)
            {
                inAir = true;
                graceTimer = graceTime;
            }
#endif
        }
#if COLLISIONX
        if (hitBox.Cast(new Vector2(Mathf.Sign(velocity.x), 0), hitResult, Mathf.Abs(velocity.x) * Time.fixedDeltaTime) > 0)
        {
            moveTo.x = hitResult[0].point.x - (hitBox.bounds.extents.x + 0.01f) * Mathf.Sign(velocity.x) - hitBox.offset.x;
            velocity.x = 0;
        }
        else
#endif
        {
            moveTo.x += velocity.x * Time.fixedDeltaTime;
        }

        #region RESET
        //Respawn the character if off screen
        if (transform.position.y < -4.0)
        {
            moveTo = spawnPoint;
            velocity.x = 0;
            velocity.y = 0;

#if PARTICLES
            resetParticle.gameObject.SetActive(true);
            resetParticle.transform.position = transform.position;
            resetParticle.Play();
#if SOUND
            resetSound.Play();
#endif
#endif
        }
        #endregion
        #endregion

        transform.position = moveTo;

        #region UPDATECONTROLTIMERS
        graceTimer -= Time.fixedDeltaTime;
        jumpBufferTimer -= Time.fixedDeltaTime;
        #endregion
    }
}
