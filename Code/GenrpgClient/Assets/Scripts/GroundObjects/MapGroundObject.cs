
using Genrpg.Shared.GroundObjects.Settings;
using Genrpg.Shared.Interactions.Messages;
using Genrpg.Shared.ProcGen.Entities;

public class MapGroundObject : InteractableObject
{

    

    public long GroundObjectId;
    public long CrafterTypeId;
    public long Level;
    public int X;
    public int Z;

    public GroundObjType GroundObj;

    protected int _useState = InteractableObject.NotUsed;
    protected override void _RightClick(float distance)
    {
        if (!CanInteract())
        {
            return;
        }

        InteractCommand interact = new InteractCommand()
        {
            TargetId = _mapObj.Id,
        };

        _networkService.SendMapMessage(interact);

    }


    protected override void _OnPointerEnter()
    {
        _cursorService.SetCursor(CursorNames.Interact);
    }
}
