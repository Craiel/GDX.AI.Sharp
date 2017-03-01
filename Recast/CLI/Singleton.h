// RecastCLI.h

#pragma once

using namespace System;

namespace RecastWrapper {

	public ref class Singleton
	{
	private:
		Singleton() {}
		Singleton(const Singleton%) { throw gcnew System::InvalidOperationException("singleton cannot be copy-constructed"); }
		static Singleton m_instance;

	public:
		static property Singleton^ Instance { Singleton^ get() { return %m_instance; } }
	};

}
