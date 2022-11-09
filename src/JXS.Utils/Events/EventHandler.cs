namespace JXS.Utils.Events;

public delegate void EventHandler<in TSender, in TArgs>(TSender sender, TArgs e) where TArgs : EventArgs;