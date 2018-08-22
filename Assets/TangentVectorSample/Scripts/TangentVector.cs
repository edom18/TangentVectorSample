using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TangentVector : MonoBehaviour
{
    [SerializeField]
    private GameObject _tangentVectorPrefab;

    [SerializeField]
    private GameObject _normalVectorPrefab;

    [SerializeField]
    private GameObject _binormalVectorPrefab;

    private GameObject _tangentVector;
    private GameObject _normalVector;
    private GameObject _binormalVector;

    private Vector3? _lastHit = null;
    private List<Vector3> _verts = new List<Vector3>();

    private Vector3[] _triangle = new Vector3[3];
    private Vector3 _n;
    private Vector3 _u;
    private Vector3 _v;
    private Vector3 _delta;

    private void GetTangetVector()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit))
        {
            return;
        }

        _verts.Clear();
        _lastHit = null;

        Vector3 p = hit.transform.InverseTransformPoint(hit.point);
        MeshFilter filter = hit.transform.GetComponent<MeshFilter>();

        Mesh mesh = filter.sharedMesh;

        _lastHit = p;

        Vector3[] nearestPoints;
        Vector2[] nearsetUVs;
        Math.GetNearestPointsInMesh(mesh, p, out nearestPoints, out nearsetUVs);

        _verts.AddRange(nearestPoints);

        for (int i = 0; i < nearestPoints.Length; i += 3)
        {
            #region 1. 同一平面上に存在する点Pが三角形内部に存在するか
            int idx0 = i + 0;
            int idx1 = i + 1;
            int idx2 = i + 2;

            Vector3 p0 = nearestPoints[idx0];
            Vector3 p1 = nearestPoints[idx1];
            Vector3 p2 = nearestPoints[idx2];

            Plane plane = new Plane(p0, p1, p2);
            Vector3 projected = plane.ClosestPointOnPlane(p);

            if (!Math.PointInTriangle(projected, p0, p1, p2))
            {
                continue;
            }
            #endregion 1. 同一平面上に存在する点Pが三角形内部に存在するか

            _triangle[0] = p0;
            _triangle[1] = p1;
            _triangle[2] = p2;
            _n = plane.normal;
            _lastHit = projected;

            #region 3. 点PのUV座標を求める
            Vector2 uv0 = nearsetUVs[idx0];
            Vector2 uv1 = nearsetUVs[idx1];
            Vector2 uv2 = nearsetUVs[idx2];

            Matrix4x4 mvp = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix * hit.transform.localToWorldMatrix;

            Vector3[] points = new[] { p0, p1, p2 };
            Vector2[] uvs = new[] { uv0, uv1, uv2 };

            Vector2 uv = Math.GetPerspectiveCollectedUV(uvs, p, points, mvp);
            #endregion 3. 点PのUV座標を求める

            #region ### 接ベクトルを計算 ###
            Vector3[] uvvector = Math.GetTangentSpaceVectors(points, uvs);

            _u = uvvector[0];
            _v = uvvector[1];
            #endregion ### 接ベクトルを計算 ###

            CreateVectorIfNeeded();

            _tangentVector.transform.position = _lastHit.Value;
            _normalVector.transform.position = _lastHit.Value;
            _binormalVector.transform.position = _lastHit.Value;

            _tangentVector.transform.forward = _v;
            _normalVector.transform.forward = _n;
            _binormalVector.transform.forward = _u;

            return;
        }
    }

    private void CreateVectorIfNeeded()
    {
        if (_tangentVector == null)
        {
            _tangentVector = Instantiate(_tangentVectorPrefab);
        }

        if (_normalVector == null)
        {
            _normalVector = Instantiate(_normalVectorPrefab);
        }

        if (_binormalVector == null)
        {
            _binormalVector = Instantiate(_binormalVectorPrefab);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GetTangetVector();
        }
    }

    private void OnDrawGizmos()
    {
        if (_lastHit == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_lastHit.Value, 0.01f);

        Gizmos.color = Color.cyan;
        for (int i = 0; i < _verts.Count; i++)
        {
            Gizmos.DrawWireSphere(_verts[i], 0.005f);
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < _triangle.Length; i++)
        {
            Gizmos.DrawLine(_triangle[i], _triangle[i] + _n * 0.05f);
        }

        Vector3 h = _lastHit.Value;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(h, h + _n * 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(h, h + _v * 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(h, h + _u * 0.1f);
    }
}
