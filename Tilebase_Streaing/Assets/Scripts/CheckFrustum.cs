using UnityEngine;

public static class CheckFrustum
{
    public enum State
    {
        Outside,
        Inside,
        Intersect,
    }

    /// <summary>
    /// Colliderバージョン（従来）
    /// </summary>
    public static State Detect(Collider target, Matrix4x4 pmat, Transform eyeTrans, float near, float far)
    {
        Plane[] planes = CalculateFrustumPlanes(pmat, eyeTrans, near, far);
        State result = State.Inside;

        for (int i = 0; i < planes.Length; i++)
        {
            Vector3 normal = planes[i].normal;
            Vector3 vp = GetPositivePoint(target.bounds, normal);
            Vector3 vn = GetNegativePoint(target.bounds, normal);

            float dp = planes[i].GetDistanceToPoint(vp);
            if (dp < 0) return State.Outside;

            float dn = planes[i].GetDistanceToPoint(vn);
            if (dn < 0) result = State.Intersect;
        }

        return result;
    }

    /// <summary>
    /// Boundsバージョン（今回使用）
    /// </summary>
    public static State Detect(Bounds bounds, Matrix4x4 pmat, Transform eyeTrans, float near, float far)
    {
        Plane[] planes = CalculateFrustumPlanes(pmat, eyeTrans, near, far);
        State result = State.Inside;

        for (int i = 0; i < planes.Length; i++)
        {
            Vector3 normal = planes[i].normal;
            Vector3 vp = GetPositivePoint(bounds, normal);
            Vector3 vn = GetNegativePoint(bounds, normal);

            float dp = planes[i].GetDistanceToPoint(vp);
            if (dp < 0) return State.Outside;

            float dn = planes[i].GetDistanceToPoint(vn);
            if (dn < 0) result = State.Intersect;
        }

        return result;
    }

    private static Vector3 GetPositivePoint(Bounds bounds, Vector3 normal)
    {
        Vector3 result = bounds.min;
        if (normal.x > 0) result.x += bounds.size.x;
        if (normal.y > 0) result.y += bounds.size.y;
        if (normal.z > 0) result.z += bounds.size.z;
        return result;
    }

    private static Vector3 GetNegativePoint(Bounds bounds, Vector3 normal)
    {
        Vector3 result = bounds.min;
        if (normal.x < 0) result.x += bounds.size.x;
        if (normal.y < 0) result.y += bounds.size.y;
        if (normal.z < 0) result.z += bounds.size.z;
        return result;
    }

    /// <summary>
    /// 視錐台の6面を Projection Matrix とカメラTransformから算出
    /// </summary>
    public static Plane[] CalculateFrustumPlanes(Matrix4x4 pmat, Transform eyeTrans, float near, float far)
    {
        Plane[] result = new Plane[6];

        for (int i = 0; i < 4; i++) // Left, Right, Bottom, Top
        {
            float a, b, c, d;
            int r = i / 2;
            if (i % 2 == 0)
            {
                a = pmat[3, 0] - pmat[r, 0];
                b = pmat[3, 1] - pmat[r, 1];
                c = pmat[3, 2] - pmat[r, 2];
                d = pmat[3, 3] - pmat[r, 3];
            }
            else
            {
                a = pmat[3, 0] + pmat[r, 0];
                b = pmat[3, 1] + pmat[r, 1];
                c = pmat[3, 2] + pmat[r, 2];
                d = pmat[3, 3] + pmat[r, 3];
            }

            Vector3 normal = -new Vector3(a, b, c).normalized;
            normal = eyeTrans.rotation * normal;
            result[i] = new Plane(normal, eyeTrans.position);
        }

        {
            float a = pmat[3, 0] + pmat[2, 0];
            float b = pmat[3, 1] + pmat[2, 1];
            float c = pmat[3, 2] + pmat[2, 2];
            float d = pmat[3, 3] + pmat[2, 3];
            Vector3 normal = -new Vector3(a, b, c).normalized;
            normal = eyeTrans.rotation * normal;
            Vector3 pos = eyeTrans.position + (eyeTrans.forward * near);
            result[4] = new Plane(normal, pos);
        }

        {
            float a = pmat[3, 0] - pmat[2, 0];
            float b = pmat[3, 1] - pmat[2, 1];
            float c = pmat[3, 2] - pmat[2, 2];
            float d = pmat[3, 3] - pmat[2, 3];
            Vector3 normal = -new Vector3(a, b, c).normalized;
            normal = eyeTrans.rotation * normal;
            Vector3 pos = eyeTrans.position + (eyeTrans.forward * near) + (eyeTrans.forward * far);
            result[5] = new Plane(normal, pos);
        }

        return result;
    }
}
