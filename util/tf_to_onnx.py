import tensorflow as tf
from tensorflow import keras
import tf2onnx
import onnx
import numpy as np


def build_onnx():
    # Register custom layer
    @tf.keras.utils.register_keras_serializable()
    class L2Normalization(tf.keras.layers.Layer):
        def call(self, inputs):
            return tf.math.l2_normalize(inputs, axis=1)

    # Load model with custom objects
    model = keras.models.load_model(
        "drawing_encoder_model.h5",
        custom_objects={'L2Normalization': L2Normalization}
    )

    print("Model loaded successfully")
    print(f"Input shape: {model.input_shape}")
    print(f"Output shape: {model.output_shape}")

    # Method 1: Direct conversion (try this first)
    try:
        input_signature = [tf.TensorSpec(model.input_shape, tf.float32, name="dist_matrix")]
        
        onnx_model, _ = tf2onnx.convert.from_keras(
            model,
            input_signature=input_signature,
            opset=13,
            output_path="drawing_encoder_model.onnx"
        )
        
        print("✓ Model successfully converted to ONNX!")
        print("✓ Saved as: drawing_encoder_model.onnx")
        
    except Exception as e:
        print(f"Method 1 failed: {e}")
        print("\nTrying Method 2...")
        
        # Method 2: Convert via SavedModel format
        try:
            # Create a concrete function
            @tf.function(input_signature=[tf.TensorSpec(shape=[None, 128, 128, 1], dtype=tf.float32)])
            def model_predict(x):
                return model(x, training=False)
            
            # Convert using the concrete function
            onnx_model, _ = tf2onnx.convert.from_function(
                model_predict,
                input_signature=[tf.TensorSpec([None, 128, 128, 1], tf.float32, name="dist_matrix")],
                opset=13,
                output_path="drawing_encoder_model.onnx"
            )
            
            print("✓ Model successfully converted to ONNX!")
            print("✓ Saved as: drawing_encoder_model.onnx")
            
        except Exception as e2:
            print(f"Method 2 also failed: {e2}")
            print("\nTrying Method 3 (SavedModel intermediate)...")
            
            # Method 3: Save as SavedModel first, then convert
            try:
                # Export to SavedModel format
                tf.saved_model.save(model, "temp_saved_model")
                print("✓ SavedModel created")
                
                # Convert SavedModel to ONNX
                onnx_model = tf2onnx.convert.from_saved_model(
                    "temp_saved_model",
                    opset=13,
                    output_path="drawing_encoder_model.onnx"
                )
                
                print("✓ Model successfully converted to ONNX!")
                print("✓ Saved as: drawing_encoder_model.onnx")
                
            except Exception as e3:
                print(f"All methods failed. Last error: {e3}")
                print("\nTry rebuilding the model without the custom layer:")
                print("See alternative solution below.")

    # Verify the ONNX model
    try:
        onnx_model = onnx.load("drawing_encoder_model.onnx")
        onnx.checker.check_model(onnx_model)
        print("\n✓ ONNX model validation passed!")
        
        # Print model info
        print("\nModel inputs:")
        for input in onnx_model.graph.input:
            print(f"  - {input.name}: {[d.dim_value for d in input.type.tensor_type.shape.dim]}")
        
        print("\nModel outputs:")
        for output in onnx_model.graph.output:
            print(f"  - {output.name}: {[d.dim_value for d in output.type.tensor_type.shape.dim]}")
            
    except Exception as e:
        print(f"\nNote: Could not verify ONNX model: {e}")

if __name__ == "__main__":
    build_onnx()