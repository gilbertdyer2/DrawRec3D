// using UnityEngine;
// using Unity.Barracuda;
// using System.Collections.Generic;

// public class GestureRecognizer : MonoBehaviour
// {
//     [Header("Model Settings")]
//     [Tooltip("Drag your .onnx model file here")]
//     public NNModel modelAsset;
    
//     [Header("Runtime Settings")]
//     [SerializeField] private WorkerFactory.Type workerType = WorkerFactory.Type.ComputePrecompiled;
    
//     private Model runtimeModel;
//     private IWorker worker;
    
//     void Start()
//     {
//         if (modelAsset == null)
//         {
//             Debug.LogError("Model asset is not assigned!");
//             return;
//         }
        
//         // Load the model
//         runtimeModel = ModelLoader.Load(modelAsset);
//         Debug.Log("Model loaded successfully");
        
//         // Create worker for inference
//         worker = WorkerFactory.CreateWorker(workerType, runtimeModel);
//         Debug.Log($"Worker created with type: {workerType}");
//     }
    

//     /// <summary>
//     /// Get embedding vector for a gesture
//     /// </summary>
//     /// <param name="points">Array of 3D points (should be 128 points)</param>
//     /// <returns>64-dimensional embedding vector</returns>
//     public float[] GetEmbedding(List<Vector3> points_l)
//     {
//         Vector3[] points = points_l.ToArray();

//         if (worker == null)
//         {
//             Debug.LogError("Worker not initialized!");
//             return null;
//         }
        
//         if (points.Length != 128)
//         {
//             Debug.LogWarning($"Expected 128 points, got {points.Length}");
//         }
        
//         // Compute distance matrix
//         float[,,,] distanceMatrix = ComputeDistanceMatrix(points);
        
//         // Create input tensor (batch=1, height=128, width=128, channels=1)
//         Tensor inputTensor = new Tensor(1, 128, 128, 1, ConvertTo1DArray(distanceMatrix));
        
//         // Run inference
//         worker.Execute(inputTensor);
        
//         // Get output tensor
//         Tensor outputTensor = worker.PeekOutput();
        
//         // Convert to float array (should be 64 values)
//         float[] embedding = new float[64];
//         for (int i = 0; i < 64 && i < outputTensor.length; i++)
//         {
//             embedding[i] = outputTensor[i];
//         }
        
//         // Clean up input tensor (output tensor is managed by worker)
//         inputTensor.Dispose();
        
//         return embedding;
//     }
    
//     /// <summary>
//     /// Compute pairwise distance matrix from 3D points
//     /// </summary>
//     private float[,,,] ComputeDistanceMatrix(Vector3[] points)
//     {
//         int n = points.Length;
//         float[,,,] matrix = new float[1, n, n, 1];
        
//         for (int i = 0; i < n; i++)
//         {
//             for (int j = 0; j < n; j++)
//             {
//                 float dist = Vector3.Distance(points[i], points[j]);
//                 matrix[0, i, j, 0] = dist;
//             }
//         }
        
//         return matrix;
//     }
    
//     /// <summary>
//     /// Convert 4D array to 1D for tensor creation
//     /// Barracuda uses NHWC layout (batch, height, width, channels)
//     /// </summary>
//     private float[] ConvertTo1DArray(float[,,,] array)
//     {
//         int batch = array.GetLength(0);
//         int height = array.GetLength(1);
//         int width = array.GetLength(2);
//         int channels = array.GetLength(3);
        
//         float[] result = new float[batch * height * width * channels];
//         int index = 0;
        
//         for (int b = 0; b < batch; b++)
//         {
//             for (int h = 0; h < height; h++)
//             {
//                 for (int w = 0; w < width; w++)
//                 {
//                     for (int c = 0; c < channels; c++)
//                     {
//                         result[index++] = array[b, h, w, c];
//                     }
//                 }
//             }
//         }
        
//         return result;
//     }
    
//     /// <summary>
//     /// Compare two embeddings and return similarity score (0-1, higher = more similar)
//     /// </summary>
//     public float CompareSimilarity(float[] embeddingA, float[] embeddingB)
//     {
//         if (embeddingA.Length != embeddingB.Length)
//         {
//             Debug.LogError("Embeddings must be same length!");
//             return 0f;
//         }
        
//         if (embeddingA.Length != embeddingB.Length)
//         {
//             Debug.LogError("Embeddings must be same length!");
//             return float.MaxValue;
//         }
        
//         // Compute Euclidean distance
//         float sumSquares = 0f;
//         for (int i = 0; i < embeddingA.Length; i++)
//         {
//             float diff = embeddingA[i] - embeddingB[i];
//             sumSquares += diff * diff;
//         }
        
//         return Mathf.Sqrt(sumSquares);
//     }
    
//     void OnDestroy()
//     {
//         // Clean up resources
//         worker?.Dispose();
//     }
// }