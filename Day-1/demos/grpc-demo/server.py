import grpc
import time
import threading
from concurrent import futures
import taskflow_analytics_pb2
import taskflow_analytics_pb2_grpc
import random

class TaskAnalyticsServicer(taskflow_analytics_pb2_grpc.TaskAnalyticsServicer):
    def __init__(self):
        # Mock data for demonstration
        self.mock_tasks = {
            "task_001": {"title": "Implement API", "status": "in_progress", "assigned_to": "alice"},
            "task_002": {"title": "Write tests", "status": "completed", "assigned_to": "bob"},
            "task_003": {"title": "Code review", "status": "pending", "assigned_to": "charlie"},
            "task_004": {"title": "Deploy to staging", "status": "overdue", "assigned_to": "diana"},
        }
        
    def GetTeamPerformance(self, request, context):
        """Unary RPC - get team performance"""
        print(f"📊 GetTeamPerformance called for team: {request.team_id}, days: {request.days_back}")
        
        # Simulate some processing time
        time.sleep(0.1)
        
        # Create mock performance data
        task_metrics = []
        for task_id, task_data in self.mock_tasks.items():
            task_metrics.append(taskflow_analytics_pb2.TaskMetric(
                task_id=task_id,
                title=task_data["title"],
                days_to_complete=random.randint(1, 10),
                complexity=random.choice(["low", "medium", "high"])
            ))
        
        return taskflow_analytics_pb2.TeamPerformance(
            team_id=request.team_id,
            completed_tasks=1,
            overdue_tasks=1,
            velocity=12.5,
            task_metrics=task_metrics
        )
    
    def StreamTaskUpdates(self, request, context):
        """Server streaming - real-time task updates"""
        print(f"🔄 StreamTaskUpdates started for team: {request.team_id}")
        
        # Simulate real-time task updates
        for i in range(5):
            if context.is_active():
                task_id = f"task_{i+1:03d}"
                update = taskflow_analytics_pb2.TaskUpdate(
                    task_id=task_id,
                    status=random.choice(["pending", "in_progress", "completed"]),
                    assigned_to=random.choice(["alice", "bob", "charlie", "diana"]),
                    timestamp=int(time.time())
                )
                print(f"📤 Streaming update: {update.task_id} -> {update.status}")
                yield update
                time.sleep(2)  # Simulate real-time updates
            else:
                break
    
    def BatchUpdateTasks(self, request_iterator, context):
        """Client streaming - batch task updates"""
        print("📦 BatchUpdateTasks started")
        
        successful_updates = 0
        failed_updates = 0
        error_messages = []
        
        for request in request_iterator:
            print(f"📝 Processing update: {request.task_id} -> {request.new_status}")
            
            # Simulate some updates failing
            if random.random() < 0.2:  # 20% failure rate
                failed_updates += 1
                error_messages.append(f"Failed to update {request.task_id}")
            else:
                successful_updates += 1
                # Update mock data
                if request.task_id in self.mock_tasks:
                    self.mock_tasks[request.task_id]["status"] = request.new_status
        
        print(f"✅ Batch complete: {successful_updates} successful, {failed_updates} failed")
        return taskflow_analytics_pb2.BatchResponse(
            successful_updates=successful_updates,
            failed_updates=failed_updates,
            error_messages=error_messages
        )
    
    def CollaborateOnTask(self, request_iterator, context):
        """Bidirectional streaming - real-time collaboration"""
        print("🤝 CollaborateOnTask started")
        
        # Process incoming collaboration messages
        for request in request_iterator:
            print(f"📥 Received collaboration: {request.user_id} - {request.action}: {request.data}")
            
            # Echo back with timestamp
            response = taskflow_analytics_pb2.TaskCollaboration(
                task_id=request.task_id,
                user_id="server",
                action="acknowledge",
                data=f"Received: {request.data}",
                timestamp=int(time.time())
            )
            yield response

def serve():
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    taskflow_analytics_pb2_grpc.add_TaskAnalyticsServicer_to_server(
        TaskAnalyticsServicer(), server
    )
    
    listen_addr = '[::]:50051'
    server.add_insecure_port(listen_addr)
    server.start()
    
    print(f"🚀 gRPC Server running on {listen_addr}")
    print("📋 Available services:")
    print("  - GetTeamPerformance (Unary)")
    print("  - StreamTaskUpdates (Server Streaming)")
    print("  - BatchUpdateTasks (Client Streaming)")
    print("  - CollaborateOnTask (Bidirectional Streaming)")
    print("\n💡 Use the client.py to test these services!")
    
    try:
        server.wait_for_termination()
    except KeyboardInterrupt:
        print("\n🛑 Shutting down server...")
        server.stop(0)

if __name__ == '__main__':
    serve() 