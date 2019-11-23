using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    [SerializeField]
    Transform _sakura;
    [SerializeField]
    Transform _rotateTarget;
    [SerializeField]
    float _horRotSpeed = 1f;
    [SerializeField]
    float _verRotSpeed = 1f;
    [SerializeField]
    [Range(30, 90)]
    float _elevationUp = 60f;
    [SerializeField]
    [Range(30, 90)]
    float _elevationDown = 60f;

    [SerializeField]
    float _moveSpeed = 1f;
    [SerializeField]
    float _nearestDis = 1.5f;
    [SerializeField]
    float _farthestDis = 8f;
    [SerializeField]
    float _highest = 0.7f;
    [SerializeField]
    float _lowest = -1f;

    float _scroll = 0f;
    float _distance = 0f;
    float _rotate = 0f;
    float _viewAngle = 0f;
    float _yOffset = 0f;

    class PosData
    {
        public float sakuraPosY;
        public float sakuraRotY;
        public float camPosX;
        public float camPosY;
        public float camPosZ;
        public float camRotX;
        public float camRotY;
        public float camRotZ;
        public float camRotW;

        public PosData(float sakuraPosY, float sakuraRotY, float camPosX, float camPosY, float camPosZ, float camRotX, float camRotY, float camRotZ, float camRotW)
        {
            this.sakuraPosY = sakuraPosY;
            this.sakuraRotY = sakuraRotY;
            this.camPosX = camPosX;
            this.camPosY = camPosY;
            this.camPosZ = camPosZ;
            this.camRotX = camRotX;
            this.camRotY = camRotY;
            this.camRotZ = camRotZ;
            this.camRotW = camRotW;
        }
    }

    void Start()
    {
        LoadPosData();
    }

    private void LateUpdate()
    {
        // 旋转中
        if (Input.GetMouseButton(1))
        {
            // 左右滑八重樱左右转，上下滑摄像机上下绕转
            _sakura.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * Time.deltaTime * _horRotSpeed * -100, 0));
            _rotate = Input.GetAxis("Mouse Y");
            // 垂直正方向与至摄像机向量角度
            _viewAngle = 180 - (Vector3.Dot(_sakura.up, transform.position - _rotateTarget.position) / Mathf.PI + 1) * 90;
            if (_rotate > 0 ?
                _viewAngle < (90 + _elevationDown) :
                _viewAngle > (90 - _elevationUp))
            {
                transform.RotateAround(_rotateTarget.position, transform.right, _rotate * Time.deltaTime * _verRotSpeed * -100);
            }
        }
        // 缩放中
        _scroll = Input.mouseScrollDelta.y;
        if (_scroll != 0)
        {
            if (Input.GetMouseButton(1))
            {
                // 高度
                _yOffset = -_sakura.position.y;
                if (_scroll > 0 ?
                _yOffset < _highest :
                _yOffset > _lowest)
                {
                    _sakura.Translate(Vector3.up * _scroll * Time.deltaTime * -5 * _moveSpeed);
                }
            }
            else
            {
                // 距离
                _distance = Vector3.Distance(transform.position, _rotateTarget.position);
                if (_scroll > 0 ? _distance > _nearestDis : _distance < _farthestDis)
                {
                    transform.Translate(Vector3.forward * _scroll * _moveSpeed * Time.deltaTime * 10, Space.Self);
                }
            }
        }
        // 松开，保存
        if (Input.GetMouseButtonUp(1))
        {
            SavePosData();
        }
    }

    void LoadPosData()
    {
        if (PlayerPrefs.HasKey(Application.productName + "_PosData"))
        {
            var str = PlayerPrefs.GetString(Application.productName + "_PosData");
            var data = JsonUtility.FromJson<PosData>(str);
            _sakura.position = new Vector3(0, data.sakuraPosY, 0);
            _sakura.rotation = Quaternion.Euler(0, data.sakuraRotY, 0);
            transform.position = new Vector3(data.camPosX, data.camPosY, data.camPosZ);
            transform.rotation = new Quaternion(data.camRotX, data.camRotY, data.camRotZ, data.camRotW);
        }
    }

    void SavePosData()
    {
        var camPos = transform.position;
        var camRot = transform.rotation;
        PosData data = new PosData(_sakura.position.y, _sakura.rotation.eulerAngles.y, camPos.x, camPos.y, camPos.z, camRot.x, camRot.y, camRot.z, camRot.w);
        var str = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(Application.productName + "_PosData", str);
    }

}
