using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;
using System;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using System.Collections;
using UnityEngine.InputSystem;

public class CharacterMove : NetworkBehaviour
{
    private Vector2 moveInput;
    public float animSpeed = 1.5f;				// アニメーション再生速度設定
    public float lookSmoother = 3.0f;			// a smoothing setting for camera motion
    public bool useCurves = true;				// Mecanimでカーブ調整を使うか設定する
    // このスイッチが入っていないとカーブは使われない
    public float useCurvesHeight = 0.5f;		// カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）

    // 以下キャラクターコントローラ用パラメタ
    // 前進速度
    public float forwardSpeed = 7.0f;
    // 後退速度
    public float backwardSpeed = 2.0f;
    // 旋回速度
    public float rotateSpeed = 2.0f;
    // ジャンプ威力
    public float jumpPower = 3.0f; 
    // キャラクターコントローラ（カプセルコライダ）の参照
    private CapsuleCollider col;
    private Rigidbody rb;
    // キャラクターコントローラ（カプセルコライダ）の移動量
    private Vector3 velocity;
    // CapsuleColliderで設定されているコライダのHeiht、Centerの初期値を収める変数
    private float orgColHight;
    private Vector3 orgVectColCenter;
    private Animator anim;							// キャラにアタッチされるアニメーターへの参照
    private AnimatorStateInfo currentBaseState;			// base layerで使われる、アニメーターの現在の状態の参照

    private GameObject cameraObject;	// メインカメラへの参照
		
    // アニメーター各ステートへの参照
    static int idleState = Animator.StringToHash ("Base Layer.Idle");
    static int locoState = Animator.StringToHash ("Base Layer.Locomotion");
    static int jumpState = Animator.StringToHash ("Base Layer.Jump");
    static int restState = Animator.StringToHash ("Base Layer.Rest");

    void Start()
    {
        // Animatorコンポーネントを取得する
        anim = GetComponent<Animator> ();
        // CapsuleColliderコンポーネントを取得する（カプセル型コリジョン）
        col = GetComponent<CapsuleCollider> ();
        rb = GetComponent<Rigidbody> ();
        //メインカメラを取得する
        cameraObject = GameObject.FindWithTag ("MainCamera");
        // CapsuleColliderコンポーネントのHeight、Centerの初期値を保存する
        orgColHight = col.height;
        orgVectColCenter = col.center;
    }

    void Update()
    {
        if (this.IsOwner)
        {
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");
            var camera = Camera.main.GetComponent<PlayerFollowCamera>();
            camera.Player = this.transform;
            SetMoveInputServerRpc(inputX, inputY);
        }

        if (this.IsServer)
        {
            Move();
            if (transform.position.y < -10.0)
            {
                transform.position = new Vector3(0, 2, 0);
            }
        }
    }

    [Unity.Netcode.ServerRpc]
    private void SetMoveInputServerRpc(float x, float y)
    {
        this.moveInput = new Vector2(x, y);
    }

    private void Move()
    {
		anim.SetFloat("Speed", moveInput.x); // Animator側で設定している"Speed"パラメタにvを渡す
		anim.SetFloat("Direction", moveInput.y); // Animator側で設定している"Direction"パラメタにhを渡す
		anim.speed = animSpeed; // Animatorのモーション再生速度に animSpeedを設定する
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0); // 参照用のステート変数にBase Layer (0)の現在のステートを設定する
		rb.useGravity = true; //ジャンプ中に重力を切るので、それ以外は重力の影響を受けるようにする



		// 以下、キャラクターの移動処理
		velocity = new Vector3(0, 0, moveInput.y); // 上下のキー入力からZ軸方向の移動量を取得
		// キャラクターのローカル空間での方向に変換
		velocity = transform.TransformDirection(velocity);
		//以下のvの閾値は、Mecanim側のトランジションと一緒に調整する
		if (moveInput.y > 0.1)
		{
			velocity *= forwardSpeed; // 移動速度を掛ける
		}
		else if (moveInput.y < -0.1)
		{
			velocity *= backwardSpeed; // 移動速度を掛ける
		}


		// 上下のキー入力でキャラクターを移動させる
		transform.localPosition += velocity * Time.fixedDeltaTime;

		// 左右のキー入力でキャラクタをY軸で旋回させる
		transform.Rotate(0, moveInput.x * rotateSpeed, 0);


		// 以下、Animatorの各ステート中での処理
		// Locomotion中
		// 現在のベースレイヤーがlocoStateの時
		if (currentBaseState.fullPathHash == locoState)
		{
			//カーブでコライダ調整をしている時は、念のためにリセットする
			if (useCurves)
			{
				resetCollider();
			}
		}
		// IDLE中の処理
		// 現在のベースレイヤーがidleStateの時
		else if (currentBaseState.fullPathHash == idleState)
		{
			//カーブでコライダ調整をしている時は、念のためにリセットする
			if (useCurves)
			{
				resetCollider();
			}

			// スペースキーを入力したらRest状態になる
			if (Input.GetButtonDown("Jump"))
			{
				anim.SetBool("Rest", true);
			}
		}
		// REST中の処理
		// 現在のベースレイヤーがrestStateの時
		else if (currentBaseState.fullPathHash == restState)
		{
			//cameraObject.SendMessage("setCameraPositionFrontView");		// カメラを正面に切り替える
			// ステートが遷移中でない場合、Rest bool値をリセットする（ループしないようにする）
			if (!anim.IsInTransition(0))
			{
				anim.SetBool("Rest", false);
			}
		}
    }
    void resetCollider ()
    {
	    // コンポーネントのHeight、Centerの初期値を戻す
	    col.height = orgColHight;
	    col.center = orgVectColCenter;
    }
}

