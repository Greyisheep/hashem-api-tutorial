import grpc
import time
import threading
import taskflow_analytics_pb2
import taskflow_analytics_pb2_grpc

def test_unary_rpc():
    """Test unary RPC - GetTeamPerformance"""
    print("\n" + "="*50)
    print("🧪 TESTING UNARY RPC (GetTeamPerformance)")
    print("="*50)
    
    # Use the Docker service name instead of localhost
    with grpc.insecure_channel('grpc-server:50051') as channel:
        stub = taskflow_analytics_pb2_grpc.TaskAnalyticsStub(channel)
        
        start_time = time.time()
        response = stub.GetTeamPerformance(
            taskflow_analytics_pb2.TeamRequest(
                team_id="eng",
                days_back=30
            )
        )
        end_time = time.time()
        
        print(f"⏱️  Response time: {(end_time - start_time)*1000:.1f}ms")
        print(f"📊 Team: {response.team_id}")
        print(f"✅ Completed tasks: {response.completed_tasks}")
        print(f"❌ Overdue tasks: {response.overdue_tasks}")
        print(f"📈 Velocity: {response.velocity}")
        print(f"📋 Task metrics: {len(response.task_metrics)} tasks")
        
        for metric in response.task_metrics[:3]:  # Show first 3
            print(f"  - {metric.title} ({metric.complexity})")

def test_server_streaming():
    """Test server streaming - StreamTaskUpdates"""
    print("\n" + "="*50)
    print("🧪 TESTING SERVER STREAMING (StreamTaskUpdates)")
    print("="*50)
    
    with grpc.insecure_channel('grpc-server:50051') as channel:
        stub = taskflow_analytics_pb2_grpc.TaskAnalyticsStub(channel)
        
        request = taskflow_analytics_pb2.TaskStreamRequest(
            team_id="eng",
            status_filter=["pending", "in_progress"]
        )
        
        print("🔄 Starting stream...")
        start_time = time.time()
        
        for update in stub.StreamTaskUpdates(request):
            elapsed = time.time() - start_time
            print(f"📤 [{elapsed:.1f}s] {update.task_id}: {update.status} -> {update.assigned_to}")
        
        print("✅ Stream completed")

def test_client_streaming():
    """Test client streaming - BatchUpdateTasks"""
    print("\n" + "="*50)
    print("🧪 TESTING CLIENT STREAMING (BatchUpdateTasks)")
    print("="*50)
    
    with grpc.insecure_channel('grpc-server:50051') as channel:
        stub = taskflow_analytics_pb2_grpc.TaskAnalyticsStub(channel)
        
        def generate_updates():
            updates = [
                ("task_001", "completed"),
                ("task_002", "in_progress"),
                ("task_003", "completed"),
                ("task_004", "pending"),
            ]
            
            for task_id, new_status in updates:
                yield taskflow_analytics_pb2.TaskUpdateRequest(
                    task_id=task_id,
                    new_status=new_status,
                    user_id="demo_user"
                )
                time.sleep(0.5)  # Simulate processing time
        
        print("📦 Sending batch updates...")
        start_time = time.time()
        response = stub.BatchUpdateTasks(generate_updates())
        end_time = time.time()
        
        print(f"⏱️  Batch processing time: {(end_time - start_time)*1000:.1f}ms")
        print(f"✅ Successful updates: {response.successful_updates}")
        print(f"❌ Failed updates: {response.failed_updates}")
        if response.error_messages:
            print("🚨 Errors:")
            for error in response.error_messages:
                print(f"  - {error}")

def test_bidirectional_streaming():
    """Test bidirectional streaming - CollaborateOnTask"""
    print("\n" + "="*50)
    print("🧪 TESTING BIDIRECTIONAL STREAMING (CollaborateOnTask)")
    print("="*50)
    
    with grpc.insecure_channel('grpc-server:50051') as channel:
        stub = taskflow_analytics_pb2_grpc.TaskAnalyticsStub(channel)
        
        def send_collaboration_messages():
            messages = [
                ("alice", "comment", "Great work on the API!"),
                ("bob", "status_change", "Moving to testing phase"),
                ("charlie", "review", "Code looks good to me"),
            ]
            
            for user_id, action, data in messages:
                request = taskflow_analytics_pb2.TaskCollaboration(
                    task_id="task_001",
                    user_id=user_id,
                    action=action,
                    data=data,
                    timestamp=int(time.time())
                )
                yield request
                time.sleep(2)
        
        print("🤝 Starting bidirectional collaboration...")
        start_time = time.time()
        
        responses = stub.CollaborateOnTask(send_collaboration_messages())
        
        for response in responses:
            elapsed = time.time() - start_time
            print(f"📥 [{elapsed:.1f}s] {response.user_id}: {response.action} - {response.data}")

def performance_comparison():
    """Compare gRPC vs REST performance (simulated)"""
    print("\n" + "="*50)
    print("📊 PERFORMANCE COMPARISON: gRPC vs REST")
    print("="*50)
    
    print("🔄 Simulating REST approach (4 separate calls):")
    rest_total = 0
    for i, endpoint in enumerate(["completed_tasks", "overdue_tasks", "velocity", "members"]):
        # Simulate REST call latency
        latency = 100 + (i * 10)  # 100-130ms per call
        rest_total += latency
        print(f"  {i+1}. GET /teams/eng/{endpoint}: ~{latency}ms")
    
    print(f"📊 REST Total: 4 requests, ~{rest_total}ms, ~15KB JSON")
    
    print("\n⚡ Simulating gRPC approach (1 call):")
    grpc_latency = 75  # Single gRPC call
    print(f"  grpc_cli call localhost:50051 GetTeamPerformance: ~{grpc_latency}ms")
    print(f"📊 gRPC Total: 1 request, ~{grpc_latency}ms, ~3KB binary")
    
    improvement = ((rest_total - grpc_latency) / rest_total) * 100
    print(f"\n🚀 Performance improvement: {improvement:.1f}% faster!")

def main():
    print("🚀 gRPC Demo Client")
    print("Testing TaskFlow Analytics Service")
    print("Connecting to grpc-server:50051")
    
    try:
        # Test all RPC types
        test_unary_rpc()
        test_server_streaming()
        test_client_streaming()
        test_bidirectional_streaming()
        
        # Show performance comparison
        performance_comparison()
        
        print("\n" + "="*50)
        print("✅ ALL TESTS COMPLETED SUCCESSFULLY!")
        print("="*50)
        print("\n💡 Key takeaways:")
        print("  - Unary RPC: Simple request-response")
        print("  - Server Streaming: Real-time updates")
        print("  - Client Streaming: Batch operations")
        print("  - Bidirectional: Real-time collaboration")
        print("  - Performance: Significantly faster than REST for complex operations")
        
    except grpc.RpcError as e:
        print(f"❌ gRPC Error: {e.code()}: {e.details()}")
        print("Make sure the server is running!")
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == '__main__':
    main() 