using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Globalization;
using System.CodeDom.Compiler;
using System.Text;
using Microsoft.CSharp;

public class RuntimeExecutor {

	private Dictionary<string,string> m_ProviderOptions = new Dictionary<string, string>()
	{
		{
			"CompilerVersion", "v3.5"
		}
	};

	private	CompilerParameters	m_CompilerParams		= new CompilerParameters { GenerateInMemory = true, GenerateExecutable = false };

	private	CSharpCodeProvider	m_CSharpCodeProvider	= null;

	private	bool				m_IsInitialized			= false;


	private	void	Initialize(  )
	{
		m_CSharpCodeProvider = new CSharpCodeProvider( m_ProviderOptions );

		m_IsInitialized = true;
	}


	private bool Execute()
	{
		if ( m_IsInitialized == false )
			return false;

		var assembly = @"
        using UnityEngine;
		
		namespace CompiledNameSpace {
 
			public class CompiledAssembly
			{
				public static void CompiledAssembly_Method()
				{
					Debug.Log(""Hello, World!"");
				}
			}
		}";


		CompilerResults results = m_CSharpCodeProvider.CompileAssemblyFromSource( m_CompilerParams, assembly );

		if ( results.Errors.Count != 0 )
		{
			foreach( CompilerError e in results.Errors )
			{
				Debug.LogError( "Error:" + e.ErrorText );
			}
			return false;
		}

		object o = results.CompiledAssembly.CreateInstance("CompiledNameSpace.CompiledAssembly");
		System.Reflection.MethodInfo mi = o.GetType().GetMethod("CompiledAssembly_Method");
		mi.Invoke( o, null );
		return true;
	}


	public static bool CompileExecutable(String sourceName)
    {
        FileInfo sourceFile = new FileInfo(sourceName);
        CodeDomProvider provider = null;
        bool compileOk = false;

        // Select the code provider based on the input file extension.
        if (sourceFile.Extension.ToUpper(CultureInfo.InvariantCulture) == ".CS")
        {
            provider = CodeDomProvider.CreateProvider("CSharp");
        }
        else if (sourceFile.Extension.ToUpper(CultureInfo.InvariantCulture) == ".VB")
        {
            provider = CodeDomProvider.CreateProvider("VisualBasic");
        }
        else 
        {
            Console.WriteLine("Source file must have a .cs or .vb extension");
        }

        if (provider != null)
        {

            // Format the executable file name.
            // Build the output assembly path using the current directory
            // and <source>_cs.exe or <source>_vb.exe.
 
            String exeName = String.Format(@"{0}\{1}.exe", 
                System.Environment.CurrentDirectory, 
                sourceFile.Name.Replace(".", "_"));

            CompilerParameters cp = new CompilerParameters();

            // Generate an executable instead of 
            // a class library.
            cp.GenerateExecutable = true;

            // Specify the assembly file name to generate.
            cp.OutputAssembly = exeName;
    
            // Save the assembly as a physical file.
            cp.GenerateInMemory = false;
    
            // Set whether to treat all warnings as errors.
            cp.TreatWarningsAsErrors = false;
 
            // Invoke compilation of the source file.
            CompilerResults cr = provider.CompileAssemblyFromFile(cp, 
                sourceName);
    
            if(cr.Errors.Count > 0)
            {
                // Display compilation errors.
                Console.WriteLine("Errors building {0} into {1}",  
                    sourceName, cr.PathToAssembly);
                foreach(CompilerError ce in cr.Errors)
                {
                    Console.WriteLine("  {0}", ce.ToString());
                    Console.WriteLine();
                }
            }
            else
            {
                // Display a successful compilation message.
                Console.WriteLine("Source {0} built into {1} successfully.",
                    sourceName, cr.PathToAssembly);
            }
              
            // Return the results of the compilation.
            if (cr.Errors.Count > 0)
            {
                compileOk = false;
            }
            else 
            {
                compileOk = true;
            }
        }
        return compileOk;
    }

}
