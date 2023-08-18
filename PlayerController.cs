using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public enum State
    {
        Standing,
        Sitting,
        Laying,
        Loading,
        Collecting,
        Jumping,
        In_car,
        Transition
    }
    // Player Vairables
    public float currentMoveSpeed;
    public float moveSpeed;
    public Vector3 velocity;
    public float currentWalkAnimationSpeed;
    public float walkAnimationSpeed;
    public float rotationSpeed;
    public float runSpeed;
    public float runAnimationSpeed;
    public float jumpDir = 0;
    public State playerState;

    // animation bools
    private bool IsSitting = false;
    private bool IsSLaying = false;
    private bool IsGrounded = false;



    //For Player Component
    public Rigidbody rb_player;
    private Animator an_Player;
    protected CapsuleCollider MainCollider;
    private float period = 0f;
    private int Collectopt;

    //For other gameobjects
    public GameObject cameraObj;
    public GameObject playerStaticRotation;
    public Transform playerSpine;
    public GameObject backpack;
    public Animator an_backpack;
    private Vector3 _lastVelocity = Vector3.zero;
    public Vector3 acceleration = Vector3.zero;
    public Vector3 angularvelocity = Vector3.zero;
    private float m_lastPressed = 0f;

    private float mouse = 0f;

    public Car NearestCar { get; protected set; }


    void Awake()
    {
        rb_player = GetComponent<Rigidbody>();
        an_Player = GetComponent<Animator>();
        playerState = State.Standing;
        MainCollider = GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       

        if (!IsGrounded) an_Player.SetBool("IsJump", false);

        if (Input.GetAxis("Vertical") != 0.0f || Input.GetAxis("Horizontal") != 0.0f)
        {
          //  MovePlayer(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        }
        MovePlayer(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));



        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            RunKeyPressed();
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunKeyUnPressed();
        }
        switch (playerState)
        {
            case State.Standing:

               if((Input.GetKeyDown(KeyCode.E) && NearestCar != null))  //I've changed it from R to E when entering a car
                {
                    Debug.Log("hey");
                    EnterCar();
               }

                if ((Input.GetKeyDown(KeyCode.LeftControl)))     //I've changed it from Enter to left ctrl
                {
                    Debug.Log("I want to sit");
                    SitDown();
                }
                
                else if ((Input.GetKeyDown(KeyCode.Tab)))
                {
                    FallDown();
                }
                else if  ((Input.GetKeyDown(KeyCode.R)))   //I've changed from L to R
                {
                    LoadToDrone();
                }
                else if((Input.GetKeyDown(KeyCode.E)))  //Changing the stopping of collecting action from C to E
                {
                    Collect();
                }
                else if ((Input.GetKeyDown(KeyCode.Space)))
                {
                    Debug.Log("I want to jump");
                    Jump();
                }
                break;


            case State.Sitting:
                if ((Input.GetKeyDown(KeyCode.LeftControl)))        //I've changed it from Enter to left ctrl
                {
                    Debug.Log("I want to get up");
                   
                    StandUp();
                }
                break;
            case State.Laying:
                if ((Input.GetKeyDown(KeyCode.Tab)))
                {
                    GetUp();
                }
                break;
            case State.Loading:
                if ((Input.GetKeyDown(KeyCode.R)))      //I've changed from L to R
                {
                    StopLoading();
                }
                break;
            case State.Collecting:
               
                if (an_Player.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("collect")) backpack.active = false;
             
                if ((Input.GetKeyDown(KeyCode.E)))      //Changing the starting of collecting action from C to E
                {   if(backpack.active == false) backpack.active = true;

                    StopCollecting();
                } 
                break;
            case State.In_car:
                this.transform.position = NearestCar.AnimDrivePosition.transform.position;
                this.transform.rotation = NearestCar.AnimDrivePosition.transform.rotation;


                float translation = Input.GetAxis("Vertical") * -6f;
                float rotation = Input.GetAxis("Horizontal") * 20f;

                translation *= Time.deltaTime;
                rotation *= Time.deltaTime;
                // Move translation along the object's z-axis
                NearestCar.transform.Translate(0, translation, 0); 
                // Rotate around our y-axis
                NearestCar.transform.Rotate(0, 0, rotation); 


              
                if ((Input.GetKeyDown(KeyCode.E) && NearestCar != null))        //I've changed it from R to E when quitting a car
                {
                  
                    EnterCar();
                }
                break;
        }
    }

    public void MovePlayer(float forward, float right)
    {
        Vector3 translation;

       
        translation = forward * cameraObj.transform.forward;
        translation += right * cameraObj.transform.right;


      
        translation.y = 0;

        if (translation.magnitude > 0.2f && (forward != 0.0f || Input.GetAxis("Horizontal") != 0.0f))
        {
            velocity = translation;
        }
        else
        {
            velocity = Vector3.zero;
            //an_Player.SetBool("IsLoad", false);
        }
        
        rb_player.velocity = new Vector3(velocity.normalized.x * moveSpeed, rb_player.velocity.y, velocity.normalized.z * moveSpeed);



        if (velocity.magnitude > 0.2f)
        {
            transform.rotation = Quaternion.Lerp(playerStaticRotation.transform.rotation, Quaternion.LookRotation(velocity), Time.deltaTime * rotationSpeed);
        }
        an_Player.SetFloat("Velocity", velocity.magnitude * walkAnimationSpeed);
       
    }

    public void RunKeyPressed()
    {
        moveSpeed = runSpeed;
        walkAnimationSpeed = runAnimationSpeed;
    }

    public void RunKeyUnPressed()
    {
        moveSpeed = currentMoveSpeed;
        walkAnimationSpeed = currentWalkAnimationSpeed;
    }

    public void SitDown()
    {
        playerState = State.Sitting;
        //an_Player.SetTrigger("SitTransition");
        an_Player.SetBool("SitDown", true);
     
    }
    public void StandUp()
    {
        playerState = State.Standing;
        //an_Player.ResetTrigger("SitTransition");
        an_Player.SetBool("SitDown", false);
    }

    public void FallDown()
    {
        playerState = State.Laying;
        an_Player.SetBool("FallDown", true);
    }
    public void GetUp()
    {
        playerState = State.Standing;
        an_Player.SetBool("FallDown", false);
    }

    public void LoadToDrone()
    {
        playerState = State.Loading;
        an_Player.SetBool("IsLoad", true);
        an_backpack.SetBool("IsLoadOpen", true);
    }
    public void StopLoading()
    {
        playerState = State.Standing;
        an_Player.SetBool("IsLoad", false);
        an_backpack.SetBool("IsLoadOpen", false);
    }
    public void Collect()
    {
        playerState = State.Collecting;
        an_Player.SetBool("IsCollect", true);
        Collectopt = MakeanewFloat();
        //an_Player.SetFloat("CollectOpt", Collectopt);
        an_Player.SetFloat("CollectOpt", 3);
        an_backpack.SetBool("IsOpen", true);


    }
    public void StopCollecting()
    {

        playerState = State.Standing;
        an_Player.SetBool("IsCollect", false);
        an_backpack.SetBool("IsOpen", false);
       
    }

    public void Jump()
    {
        an_Player.SetBool("IsJump", true);
        IsGrounded = false;
    }

    public void EnterCar()
    {
        switch (NearestCar.State)
        {
            case Car.CarState.FREE:
                if (NearestCar != null && playerState == State.Standing && NearestCar.State == Car.CarState.FREE)
                {
                    playerState = State.Transition;
                    NearestCar.State = Car.CarState.OCCUPIED;
                    StartCoroutine(EnterCarAnimation());
                }
                break;
            case Car.CarState.OCCUPIED:
                if (playerState == State.In_car)
                {
                   
                    playerState = State.Transition;
                    NearestCar.State = Car.CarState.FREE;
                    StartCoroutine(ExitCarAnimation());
                }
                break;
        }

        }
    public IEnumerator EnterCarAnimation()
    {
        var time = 0f;
        an_Player.SetTrigger("EnterCar");
        rb_player.useGravity = false;
        const float animTime = 5f;
        MainCollider.enabled = false;
       

        

        while (time < animTime)
        {
            
            an_Player.SetBool("InCar", true);
            NearestCar.Animator.SetBool("OpenDoor", 0.3f < time && time < 4f);
            this.transform.position = Vector3.Lerp(NearestCar.AnimEnterPosition.transform.position, NearestCar.AnimDrivePosition.transform.position,time/5f);
            //this.transform.rotation = Quaternion.Lerp(NearestCar.AnimEnterPosition.transform.rotation,NearestCar.AnimDrivePosition.transform.rotation, time /5f);
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
       
        playerState = State.In_car;
    }

    public IEnumerator ExitCarAnimation()
    {
        //get out
        var time = 0f;
        an_Player.SetBool("InCar", false);
        const float animTime = 5f;
        while (time < animTime)
        {
            NearestCar.Animator.SetBool("OpenDoor", 0.0f < time && time < 4f);
            transform.position = Vector3.Lerp(NearestCar.AnimDrivePosition.transform.position, NearestCar.AnimEnterPosition.transform.position, time / 3f);
            transform.rotation = Quaternion.Lerp(NearestCar.AnimDrivePosition.transform.rotation, NearestCar.AnimEnterPosition.transform.rotation, time / 3f);
            time += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb_player.useGravity = true;
        MainCollider.enabled = true;
        playerState = State.Standing;
    }

    public int MakeanewFloat()
    {
        return (int)Random.Range(1, 5f);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (NearestCar == null && other.GetComponent<Car>() != null)
        {
            NearestCar = other.GetComponent<Car>();
           // Debug.Log("next to car");
        }
           
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Car>() == NearestCar)
            NearestCar = null;
    }

}