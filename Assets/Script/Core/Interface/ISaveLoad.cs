using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

public interface ISaveLoad
{
    string[] Save();

    void Load(string[] data);

    string GetId();
}
