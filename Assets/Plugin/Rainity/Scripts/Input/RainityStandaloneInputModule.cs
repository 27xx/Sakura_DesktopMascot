using System;
using System.Collections.Generic;

namespace UnityEngine.EventSystems
{
	public class RainityStandaloneInputModule : StandaloneInputModule
	{
		new void Awake() {
			m_InputOverride = this.gameObject.AddComponent<RainityBaseInput>();
		}

		void Update() {
		}

		public override bool ShouldActivateModule() {
			return true;
		}

		public override bool IsModuleSupported() {
			return true;
		}

		public override void DeactivateModule() {
			
		}
	}
}