#nullable enable


namespace ILL_HSceneBreastJiggle
{
	public static class Mathf
	{
		public static float Clamp01(float value)
		{
			if (value < 0f) return 0f;
			else if (value > 1f) return 1f;
			else return value;
		}
	}
}