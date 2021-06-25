namespace SupportApi.Collaboration
{

    /// <summary>
    /// 共有ドキュメントの変更タイプ
    /// </summary>
    public enum ModificationType
    {
        NoChanges = 0,
        Structure = 1,
        RemoveAnnotation = 2,
        AddAnnotation = 3,
        UpdateAnnotation = 4,
        Undo = 5,
        Redo = 6,
        ResetUndo = 7,
        Reset = 8,
    }

}