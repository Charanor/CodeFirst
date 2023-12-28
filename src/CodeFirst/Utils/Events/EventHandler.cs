namespace CodeFirst.Utils.Events;

public delegate void EventHandler<in TSender, in TArgs>(TSender sender, TArgs e);
public delegate void EventHandler<in TSender>(TSender sender);