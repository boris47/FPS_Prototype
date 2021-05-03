
/// <summary> </summary>
/// <typeparam name="K">The derive class or the implemented interface</typeparam>
public interface IAttachments<K>
{
	T						AddAttachment<T>				() where T : K, new();
	bool					HasAttachment<T>				() where T : K, new();
	T						GetAttachment<T>				() where T : K, new();
	void					RemoveAttachment<T>				() where T : K, new();
	void					ToggleAttachment<T>				() where T : K, new();
	void					ActivateAttachment<T>			() where T : K, new();
	void					DeactivateAttachment<T>			() where T : K, new();

	K						AddAttachment					(System.Type type);
	bool					HasAttachment					(System.Type type);
	K						GetAttachment					(System.Type type);
	void					RemoveAttachment				(System.Type type);
	void					ToggleAttachment				(System.Type type);
	void					ActivateAttachment				(System.Type type);
	void					DeactivateAttachment			(System.Type type);

	void					ActivateAllAttachments			();
	void					DeactivateAllAttachments		();
	void					RemoveAllAttachments			();
	void					ResetAttachments				();
}