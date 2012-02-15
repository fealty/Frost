namespace Frost.Shaping
{
	public interface ITessellationSink
	{
		void Begin();
		void AddTriangle(Point p1, Point p2, Point p3);
		void End();
	}
}