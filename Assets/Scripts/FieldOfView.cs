using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creats a field of view based on the length and angle of view
/// Stops when it encounters an obstacle
/// Enables a "target found" tag when a target is within view
/// </summary>
public class FieldOfView : MonoBehaviour
{
    /// <summary>
    /// Distance from center of the unit the view extends to
    /// </summary>
    public float viewRadius;

    /// <summary>
    /// Angle from the center of the unit that degines the width of the angle
    /// </summary>
    [Range(0, 360)]
    public float viewAngle;

    /// <summary>
    /// Collision masks
    /// </summary>
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    /// <summary>
    /// Keeps a list of all targets in view
    /// </summary>
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    /// <summary>
    /// How long to wait between cycles to re-create visible targets list
    /// </summary>
    public float findTargetDelay = .25f;

    /// <summary>
    /// Total rays to cast to create the field of view visual
    /// </summary>
    public float meshResolution;

    /// <summary>
    /// Total iterations to run while attempting to find the edge of an object obstracting the view
    /// </summary>
    public int edgeResolveIteration;

    /// <summary>
    /// When comparing if two rays are hitting the same object, this treshold defines the distance
    /// at which we will consider them to be two distinct surfaces or objects
    /// </summary>
    public float edgeDistanceTreshold;

    /// <summary>
    /// Contains the view mesh
    /// </summary>
    public MeshFilter viewMeshFilter;

    /// <summary>
    /// The mesh that represents the view
    /// </summary>
    public Mesh viewMesh;

    /// <summary>
    /// Triggers the visible target finder
    /// </summary>
    void Start()
    {
        this.viewMesh = new Mesh();
        this.viewMesh.name = "ViewMesh";
        this.viewMeshFilter.mesh = this.viewMesh;

        StartCoroutine("FindTargetsWithDelay", this.findTargetDelay);
    }

    /// <summary>
    /// Draws the field of view
    /// </summary>
    void LateUpdate()
    {
        this.DrawFieldOfView();
    }

    /// <summary>
    /// Runs throughout the application looking for visible targets
    /// Waits betweens cycles based on the given delay
    /// </summary>
    /// <param name="delay"></param>
    /// <returns></returns>
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while(true) {
            yield return new WaitForSeconds(delay);
            this.FindVisibleTargets();
        }
    }

    /// <summary>
    /// Grabs all colliders within the view radius
    /// Chooses only the ones within the view angle
    /// Removes all colliders blocked by the obstacle mask
    /// </summary>
    void FindVisibleTargets()
    {
        this.visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, this.viewRadius, this.targetMask);

        for(int i = 0; i < targetsInViewRadius.GetLength(0); i++) {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 directionToTarget = (target.position - this.transform.position).normalized;

            // Within angle of view
            if(Vector3.Angle(transform.forward, directionToTarget) < this.viewAngle / 2){

                // Make sure there's no obstacle between this object and the target
                float distanceToTarget = Vector3.Distance(this.transform.position, target.position);

                if( !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, this.obstacleMask) ) {
                    Debug.Log(target.name + " is in view");
                    this.visibleTargets.Add(target);
                }
            }
        } // for
    }

    /// <summary>
    /// Returns a Vector3 that represents the direction an angle is facing in world space
    /// </summary>
    /// <param name="angleInDregrees"></param>
    /// <returns>World Space Direction</returns>
    public Vector3 DirectionFromAngle(float angleInDregrees, bool angleIsGlobal)
    {
        // Convert to global angle
        if(!angleIsGlobal) {
            angleInDregrees += transform.eulerAngles.y;
        }

        // By swapping cosign and sign we transform unity's angle into trig's version
        return new Vector3(
            Mathf.Sin(angleInDregrees * Mathf.Deg2Rad), 
            0f, 
            Mathf.Cos(angleInDregrees * Mathf.Deg2Rad)
        );
    }

    /// <summary>
    /// Casts rays in the shape of the view radious and angle
    /// Starting from the left most angle all the way to the right most angle
    /// Generates a mesh between these rays to create a visual representation of the field of vision
    /// </summary>
    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(this.viewAngle * this.meshResolution);
        float stepAngleSize = this.viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo previousViewCastInfo = new ViewCastInfo();

        for(int i = 0; i < stepCount; i++) {
            float angle = this.transform.eulerAngles.y - this.viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo viewCastInfo = this.ViewCast(angle);

            // Checking if between the previous and current iteration
            // there's been a hit and miss scenario so that we can find
            // the edge of the obstacle encountered
            if(i > 0) {
                bool edgeDistanceTresholdExceeded = Mathf.Abs(previousViewCastInfo.distance - viewCastInfo.distance) > this.edgeDistanceTreshold;
                bool findEdge = previousViewCastInfo.hit && viewCastInfo.hit && edgeDistanceTresholdExceeded;

                if(previousViewCastInfo.hit != viewCastInfo.hit || findEdge) {
                    EdgeInfo edge = this.FindEdge(previousViewCastInfo, viewCastInfo);
                    if(edge.pointA != Vector3.zero){
                        viewPoints.Add(edge.pointA);
                    }
                    if(edge.pointB != Vector3.zero){
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(viewCastInfo.point);
            previousViewCastInfo = viewCastInfo;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        // Origin
        vertices[0] = Vector3.zero;

        // Generate the remaining (triangle points) a.k.a "vertices
        for(int i = 0; i < vertexCount - 1; i++) {
            vertices[i + 1] = this.transform.InverseTransformPoint(viewPoints[i]);

            if( i < vertexCount - 2) {
                // Each triangle starts at "origin:
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }            
        } // for

        // Re-draw the mesh
        this.viewMesh.Clear();
        this.viewMesh.vertices = vertices;
        this.viewMesh.triangles = triangles;
        this.viewMesh.RecalculateNormals();
    }

    /// <summary>
    /// Performs a raycast from the center of the unit in the angle given up to the view radius
    /// Collects data when collision with the obstacle layer happens or not
    /// Returns information about the raycast made such as distance covered and wether collision occurred or not
    /// </summary>
    /// <param name="globalAngle"></param>
    /// <returns></returns>
    ViewCastInfo ViewCast(float globalAngle)
    {
        RaycastHit hit;
        Vector3 direction = this.DirectionFromAngle(globalAngle, true);
        bool isObstructed = Physics.Raycast(this.transform.position,
                                            direction, 
                                            out hit,
                                            this.viewRadius, 
                                            this.obstacleMask);
        if(isObstructed) {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        
        return new ViewCastInfo(false, this.transform.position + direction * this.viewRadius, this.viewRadius, globalAngle);
    }

    /// <summary>
    /// Iterates through the <see cref="edgeResolveIteration"/> attempting to locate
    /// the edge of an object obstructing the view
    /// </summary>
    /// <param name="minViewCast"></param>
    /// <param name="maxViewCast"></param>
    /// <returns></returns>
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        // Create raycasts updated the min and maximum each time a hit or miss is found
        for(int i = 0; i < this.edgeResolveIteration; i++) {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCastInfo = this.ViewCast(angle);

            bool edgeDistanceTresholdExceeded = Mathf.Abs(minViewCast.distance - maxViewCast.distance) > this.edgeDistanceTreshold;

            if(newViewCastInfo.hit == minViewCast.hit && !edgeDistanceTresholdExceeded) {
                minAngle = angle;
                minPoint = newViewCastInfo.point;
            } else {
                maxAngle = angle;
                maxPoint = newViewCastInfo.point;
            }
        } // for

        return new EdgeInfo(minPoint, maxPoint);
    }

    /// <summary>
    /// Container for the all the information for when we raycast to generate the field of view
    /// </summary>
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle)
        {
            this.hit = hit;
            this.point = point;
            this.distance = distance;
            this.angle = angle;
        }
    }

    /// <summary>
    /// Contains the two points that help define where the edge of an object exists 
    /// This is so that we can draw an extra ray btween the ray that hit an objects last
    /// and the ray that missed the obstacle after this one
    /// </summary>
    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            this.pointA = pointA;
            this.pointB = pointB;
        }
    }
}
