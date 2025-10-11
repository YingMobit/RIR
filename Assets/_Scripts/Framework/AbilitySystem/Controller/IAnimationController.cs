namespace GAS {
    internal interface IAnimationController : IController {
        public void SetFloat(string name,float value);
        public void SetBool(string name,bool value);
        public void SetFloatSmooth(string name,float value,float smoothTime);
    }
}