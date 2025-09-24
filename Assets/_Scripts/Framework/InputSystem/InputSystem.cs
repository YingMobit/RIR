using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace InputSystem {
    public class InputSystem {
        private InputMappingConfig configCache;
        private InputMappingConfig Config{ 
            get{
                if(configCache == null)
                    configCache = Resources.Load<InputMappingConfig>(InputMappingConfig.AssetPath);
                return configCache;
            }
        }
        public void OnUpdate(int frameCount) { 
            //这里后续ECS系统搭建好之后处理本地的和网络代理的InputBufferComponent的输入数据,暂时只处理本地

        }
    }
}
