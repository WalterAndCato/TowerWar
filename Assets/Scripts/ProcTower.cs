using System.Collections.Generic;
using UnityEngine;

public class ProcTower : MonoBehaviour
{
    public GameObject brick;
    public Rigidbody2D floor;
    public int layers = 16;
    public int bricksPerLayer = 4;
    public float breakingForce = 1000;
    public float breakingTorque = 5000;
    public float breakingForceFloor = 10000;
    public float breakingTorqueFloor = 50000;

    private void Start()
    {
        var split = false;
        var lastLayerObjs = new List<GameObject>();
        var currLayerObjs = new List<GameObject>();

        for (var i = 0; i < layers; i++)
        {
            for (var j = 0; j < bricksPerLayer; j++)
            {
                if (split && j == 0)
                {
                    var brick = Instantiate(this.brick, transform);
                    brick.transform.localPosition = new Vector3(0.25f, 0.2f + i * 0.4f);
                    brick.transform.localScale = new Vector3(0.5f, 0.4f);

                    var joint = brick.AddComponent<TowerJoint>();
                    joint.breakingForce = breakingForce;
                    joint.breakingTorque = breakingTorque;

                    if (i != 0)
                    {
                        joint.joins.Add(new TowerJoint.JoinDefinition
                        {
                            anchorLocalSpace = new Vector3(0, -0.5f),
                            connectedBody = lastLayerObjs[0].GetComponent<Rigidbody2D>()
                        });
                    }
                    else
                    {
                        joint.breakingTorque = breakingTorqueFloor;
                        joint.breakingForce = breakingForceFloor;

                        joint.joins.Add(new TowerJoint.JoinDefinition
                        {
                            anchorLocalSpace = new Vector3(0, -0.5f),
                            connectedBody = floor
                        });
                    }

                    currLayerObjs.Add(brick);
                }
                else if (split && j == bricksPerLayer - 1)
                {
                    var brick = Instantiate(this.brick, transform);
                    brick.transform.localPosition = new Vector3(1 + j - 0.25f, 0.2f + i * 0.4f);
                    brick.transform.localScale = new Vector3(0.5f, 0.4f);
                    var joint = brick.AddComponent<TowerJoint>();
                    joint.breakingForce = breakingForce;
                    joint.breakingTorque = breakingTorque;

                    joint.joins.Add(new TowerJoint.JoinDefinition
                    {
                        anchorLocalSpace = new Vector3(-0.5f, 0),
                        connectedBody = currLayerObjs[^1].GetComponent<Rigidbody2D>()
                    });

                    if (i != 0)
                    {
                        joint.joins.Add(new TowerJoint.JoinDefinition
                        {
                            anchorLocalSpace = new Vector3(0, -0.5f),
                            connectedBody = lastLayerObjs[^1].GetComponent<Rigidbody2D>()
                        });
                    }
                    else
                    {
                        var jt = brick.AddComponent<TowerJoint>();
                        jt.breakingForce = breakingForceFloor;
                        jt.breakingTorque = breakingTorqueFloor;

                        jt.joins.Add(new TowerJoint.JoinDefinition
                        {
                            anchorLocalSpace = new Vector3(0, -0.5f),
                            connectedBody = floor
                        });
                    }

                    currLayerObjs.Add(brick);
                    continue;
                }

                {
                    var brick = Instantiate(this.brick, transform);
                    brick.transform.localPosition = new Vector3((split ? 1 : 0.5f) + j, 0.2f + i * 0.4f);

                    var joint = brick.AddComponent<TowerJoint>();
                    joint.breakingForce = breakingForce;
                    joint.breakingTorque = breakingTorque;

                    if (j != 0)
                    {
                        joint.joins.Add(new TowerJoint.JoinDefinition
                        {
                            anchorLocalSpace = new Vector3(-0.5f, 0),
                            connectedBody = currLayerObjs[^1].GetComponent<Rigidbody2D>()
                        });
                    }

                    if (i != 0)
                    {
                        joint.joins.Add(new TowerJoint.JoinDefinition
                        {
                            anchorLocalSpace = new Vector3(-0.25f, -0.5f),
                            connectedBody = lastLayerObjs[j].GetComponent<Rigidbody2D>()
                        });

                        joint.joins.Add(new TowerJoint.JoinDefinition
                        {
                            anchorLocalSpace = new Vector3(0.25f, -0.5f),
                            connectedBody = lastLayerObjs[j + 1].GetComponent<Rigidbody2D>()
                        });
                    }
                    else
                    {
                        var jt = j == 0 ? joint : brick.AddComponent<TowerJoint>();
                        jt.breakingForce = breakingForceFloor;
                        jt.breakingTorque = breakingTorqueFloor;

                        jt.joins.Add(new TowerJoint.JoinDefinition
                        {
                            anchorLocalSpace = new Vector3(0, -0.5f),
                            connectedBody = floor
                        });
                    }

                    currLayerObjs.Add(brick);
                }
            }

            split = !split;
            (lastLayerObjs, currLayerObjs) = (currLayerObjs, lastLayerObjs);
            currLayerObjs.Clear();
        }

        if (layers % 2 == 0)
        {
            // Letzte Reihe ist split
            var brick1 = Instantiate(brick, transform);
            brick1.transform.localPosition = new Vector3(0.5f, layers * 0.4f + 0.2f);
            var joint1 = brick1.AddComponent<TowerJoint>();
            joint1.breakingForce = breakingForce;
            joint1.breakingTorque = breakingTorque;

            joint1.joins.Add(new TowerJoint.JoinDefinition
            {
                anchorLocalSpace = new Vector3(-0.25f, -0.5f),
                connectedBody = lastLayerObjs[0].GetComponent<Rigidbody2D>()
            });

            joint1.joins.Add(new TowerJoint.JoinDefinition
            {
                anchorLocalSpace = new Vector3(0.25f, -0.5f),
                connectedBody = lastLayerObjs[1].GetComponent<Rigidbody2D>()
            });

            var brick2 = Instantiate(brick, transform);
            brick2.transform.localPosition = new Vector3(bricksPerLayer - 0.5f, layers * 0.4f + 0.2f);
            var joint2 = brick2.AddComponent<TowerJoint>();
            joint2.breakingForce = breakingForce;
            joint2.breakingTorque = breakingTorque;

            joint2.joins.Add(new TowerJoint.JoinDefinition
            {
                anchorLocalSpace = new Vector3(-0.25f, -0.5f),
                connectedBody = lastLayerObjs[^2].GetComponent<Rigidbody2D>()
            });

            joint2.joins.Add(new TowerJoint.JoinDefinition
            {
                anchorLocalSpace = new Vector3(0.25f, -0.5f),
                connectedBody = lastLayerObjs[^1].GetComponent<Rigidbody2D>()
            });
        }
        else
        {
            // Letzte Reihe ist kein split
            var brick1 = Instantiate(brick, transform);
            brick1.transform.localPosition = new Vector3(0.25f, layers * 0.4f + 0.2f);
            brick1.transform.localScale = new Vector3(0.5f, 0.4f);
            var joint1 = brick1.AddComponent<TowerJoint>();
            joint1.breakingForce = breakingForce;
            joint1.breakingTorque = breakingTorque;

            joint1.joins.Add(new TowerJoint.JoinDefinition
            {
                anchorLocalSpace = new Vector3(-0.5f, 0),
                connectedBody = lastLayerObjs[0].GetComponent<Rigidbody2D>()
            });

            var brick2 = Instantiate(brick, transform);
            brick2.transform.localPosition = new Vector3(0.875f + bricksPerLayer - 1.25f, layers * 0.4f + 0.2f);
            brick2.transform.localScale = new Vector3(0.5f, 0.4f);
            var joint2 = brick2.AddComponent<TowerJoint>();
            joint2.breakingForce = breakingForce;
            joint2.breakingTorque = breakingTorque;

            joint2.joins.Add(new TowerJoint.JoinDefinition
            {
                anchorLocalSpace = new Vector3(-0.5f, 0),
                connectedBody = lastLayerObjs[^1].GetComponent<Rigidbody2D>()
            });
        }
    }
}
