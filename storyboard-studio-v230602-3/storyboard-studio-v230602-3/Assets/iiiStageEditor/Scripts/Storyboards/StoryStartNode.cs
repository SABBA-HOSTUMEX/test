
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RuntimeNodeEditor;
using UnityEngine.UI;
using Battlehub.RTSL.Interface;


public class StoryStartNode : Node
{
    public TMP_InputField valueField;   //  added from editor
    public SocketOutput outputSocket;   //  added from editor
    // Start is called before the first frame update
    public override void Setup()
    {
        Register(outputSocket);
        //SetHeader("開始");
    }

    public override void OnSerialize(Serializer serializer)
    {
        //  save values on graph save
        //serializer.Add("floatValue", valueField.text);

        //  it would be good idea to use JsonUtility for complex data
    }

    public override void OnDeserialize(Serializer serializer)
    {
        //  load values on graph load
        //var value = serializer.Get("floatValue");
        //valueField.SetTextWithoutNotify(value);


        StoryBoardNodeEditor nodeEditor = gameObject.GetComponentInParent<StoryBoardNodeEditor>();
        if (nodeEditor != null)
        {

            nodeEditor.StartNode = this;
        }
    }

}